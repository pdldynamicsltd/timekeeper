using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Abp.Localization;
using CadentManagement.RateLimiting;

namespace CadentManagement.Web.RateLimiting;

public static class DynamicRateLimitingExtensions
{
    private const int PasswordResetWindowInMinutes = 15;

    public static IServiceCollection AddDynamicRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                await WriteRateLimitRejectionResponse(context.HttpContext, cancellationToken);
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var cacheManager = httpContext.RequestServices.GetService<IRateLimitPolicyCacheManager>();

                if (cacheManager == null)
                {
                    return RateLimitPartition.GetNoLimiter(string.Empty);
                }

                var isEnabled = cacheManager.IsRateLimitingEnabled();
                if (!isEnabled)
                {
                    return RateLimitPartition.GetNoLimiter(string.Empty);
                }

                var policies = cacheManager.GetActivePolicies();
                if (policies == null || policies.Count == 0)
                {
                    return RateLimitPartition.GetNoLimiter(string.Empty);
                }

                var requestPath = httpContext.Request.Path.ToString();
                var matchingPolicy = FindMatchingPolicy(policies, requestPath);

                if (matchingPolicy == null)
                {
                    return RateLimitPartition.GetNoLimiter(string.Empty);
                }

                var partitionKey = GetPartitionKey(httpContext, matchingPolicy);

                return CreateRateLimitPartition(matchingPolicy, partitionKey);
            });

            options.AddPolicy("PasswordlessLoginLimiter", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromSeconds(60)
                    }
                )
            );

            // Rate-limit password reset attempts to prevent brute-force attacks
            options.AddPolicy("PasswordResetLimiter", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(PasswordResetWindowInMinutes)
                    }
                )
            );
        });

        return services;
    }

    private static RateLimitPolicy FindMatchingPolicy(
        System.Collections.Generic.List<RateLimitPolicy> policies,
        string requestPath)
    {
        // First try to match specific endpoint policies
        var endpointPolicy = policies
            .Where(p => !p.IsGlobal && !string.IsNullOrEmpty(p.EndpointPattern))
            .FirstOrDefault(p => MatchesEndpointPattern(requestPath, p.EndpointPattern));

        if (endpointPolicy != null)
        {
            return endpointPolicy;
        }

        // Fall back to global policies (return first enabled global)
        return policies.FirstOrDefault(p => p.IsGlobal);
    }

    private static bool MatchesEndpointPattern(string requestPath, string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            return false;
        }

        // Convert wildcard pattern to regex
        // e.g., "/api/services/app/User/*" -> "^/api/services/app/User/.*$"
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
        return Regex.IsMatch(requestPath, regexPattern, RegexOptions.IgnoreCase);
    }

    private static string GetPartitionKey(HttpContext httpContext, RateLimitPolicy policy)
    {
        var policyPrefix = $"policy_{policy.Id}_";

        return policy.PartitionType switch
        {
            RateLimitPartitionType.ByUser =>
                policyPrefix + (httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous"),
            RateLimitPartitionType.ByApiKey =>
                policyPrefix + (httpContext.Request.Headers["X-API-Key"].ToString() is { Length: > 0 } apiKey ? apiKey : "no-key"),
            _ => // ByClientIp
                policyPrefix + (httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown")
        };
    }

    private static RateLimitPartition<string> CreateRateLimitPartition(RateLimitPolicy policy, string partitionKey)
    {
        return policy.Algorithm switch
        {
            RateLimitAlgorithm.SlidingWindow => RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey,
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = policy.PermitLimit,
                    Window = TimeSpan.FromSeconds(policy.WindowInSeconds),
                    SegmentsPerWindow = policy.SegmentsPerWindow > 0 ? policy.SegmentsPerWindow : 1,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = policy.QueueLimit
                }),

            RateLimitAlgorithm.TokenBucket => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey,
                _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = policy.PermitLimit,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(
                        policy.ReplenishmentPeriodInSeconds > 0 ? policy.ReplenishmentPeriodInSeconds : 1),
                    TokensPerPeriod = policy.TokensPerPeriod > 0 ? policy.TokensPerPeriod : 1,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = policy.QueueLimit,
                    AutoReplenishment = true
                }),

            RateLimitAlgorithm.Concurrency => RateLimitPartition.GetConcurrencyLimiter(
                partitionKey,
                _ => new ConcurrencyLimiterOptions
                {
                    PermitLimit = policy.PermitLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = policy.QueueLimit
                }),

            _ => RateLimitPartition.GetFixedWindowLimiter( // FixedWindow (default)
                partitionKey,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = policy.PermitLimit,
                    Window = TimeSpan.FromSeconds(policy.WindowInSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = policy.QueueLimit,
                    AutoReplenishment = true
                })
        };
    }

    private static async Task WriteRateLimitRejectionResponse(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var cacheManager = httpContext.RequestServices.GetService<IRateLimitPolicyCacheManager>();
        var requestPath = httpContext.Request.Path.ToString();

        string message = null;
        if (cacheManager != null)
        {
            var policies = cacheManager.GetActivePolicies();
            var matchingPolicy = policies != null ? FindMatchingPolicy(policies, requestPath) : null;
            if (!string.IsNullOrWhiteSpace(matchingPolicy?.CustomMessage))
            {
                message = matchingPolicy.CustomMessage;
            }
        }

        if (string.IsNullOrEmpty(message))
        {
            var localizationManager = httpContext.RequestServices.GetService<ILocalizationManager>();
            message = localizationManager?.GetString("CadentManagement", "TooManyRequestsMessage");
        }

        var body = JsonSerializer.Serialize(new
        {
            __abp = true,
            success = false,
            error = new
            {
                code = 429,
                message,
                details = (string)null
            },
            unAuthorizedRequest = false
        });

        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsync(body, cancellationToken);
    }
}
