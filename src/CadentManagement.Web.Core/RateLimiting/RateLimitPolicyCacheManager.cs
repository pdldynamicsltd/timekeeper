using System.Collections.Generic;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using CadentManagement.Configuration;
using CadentManagement.RateLimiting;

namespace CadentManagement.Web.RateLimiting;

public class RateLimitPolicyCacheManager : IRateLimitPolicyCacheManager, ITransientDependency
{
    private const string CacheName = "AppRateLimitPolicies";
    private const string ActivePoliciesKey = "ActivePolicies";
    private const string IsEnabledKey = "IsEnabled";

    private readonly ICacheManager _cacheManager;
    private readonly IRepository<RateLimitPolicy> _repository;
    private readonly ISettingManager _settingManager;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IRateLimitCacheInvalidator _cacheInvalidator;

    public RateLimitPolicyCacheManager(
        ICacheManager cacheManager,
        IRepository<RateLimitPolicy> repository,
        ISettingManager settingManager,
        IUnitOfWorkManager unitOfWorkManager,
        IRateLimitCacheInvalidator cacheInvalidator)
    {
        _cacheManager = cacheManager;
        _repository = repository;
        _settingManager = settingManager;
        _unitOfWorkManager = unitOfWorkManager;
        _cacheInvalidator = cacheInvalidator;
    }

    public bool IsRateLimitingEnabled()
    {
        var cache = _cacheManager
            .GetCache(CacheName)
            .AsTyped<string, string>();

        var value = cache.Get(IsEnabledKey, _ =>
        {
            return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                return _settingManager.GetSettingValueForApplication(AppSettings.RateLimiting.IsEnabled);
            });
        });

        return bool.Parse(value);
    }

    public List<RateLimitPolicy> GetActivePolicies()
    {
        var cache = _cacheManager
            .GetCache(CacheName)
            .AsTyped<string, List<RateLimitPolicy>>();

        return cache.Get(ActivePoliciesKey, _ =>
        {
            return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                return _repository.GetAllList(p => p.IsEnabled);
            });
        });
    }
}
