using System.Collections.Generic;
using CadentManagement.RateLimiting;

namespace CadentManagement.Web.RateLimiting;

public interface IRateLimitPolicyCacheManager
{
    List<RateLimitPolicy> GetActivePolicies();

    bool IsRateLimitingEnabled();
}
