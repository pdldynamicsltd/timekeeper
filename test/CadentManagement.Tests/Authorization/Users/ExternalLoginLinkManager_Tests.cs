using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using CadentManagement.Authorization.Users.ExternalLoginLink;
using Shouldly;
using Xunit;

namespace CadentManagement.Tests.Authorization.Users;

// ReSharper disable once InconsistentNaming
public class ExternalLoginLinkManager_Tests : AppTestBase
{
    private readonly IExternalLoginLinkManager _externalLoginLinkManager;

    public ExternalLoginLinkManager_Tests()
    {
        _externalLoginLinkManager = Resolve<IExternalLoginLinkManager>();
    }

    [Fact]
    public async Task Should_Remove_Merge_Token_After_Successful_Server_Side_Merge()
    {
        var otherUserId = await CreateUserAndGetId("tokenmergeuser", "2mF9d8Ac!5");
        UsingDbContext(context =>
        {
            context.UserLogins.Add(new UserLogin
            {
                LoginProvider = "Google",
                ProviderKey = "google-merge-token-key",
                TenantId = AbpSession.TenantId,
                UserId = otherUserId
            });
            context.SaveChanges();
        });

        var mergeToken = await _externalLoginLinkManager.CreateMergeTokenAsync(
            AbpSession.UserId.Value,
            AbpSession.TenantId,
            "Google",
            "google-merge-token-key",
            "google-merge-token@test.com");

        await _externalLoginLinkManager.MergeAndLinkExternalLoginAsync(
            AbpSession.UserId.Value,
            AbpSession.TenantId,
            mergeToken,
            "2mF9d8Ac!5");

        await UsingDbContextAsync(async context =>
        {
            var login = await context.UserLogins.FirstOrDefaultAsync(ul =>
                ul.LoginProvider == "Google" && ul.ProviderKey == "google-merge-token-key");
            login.ShouldNotBeNull();
            login.UserId.ShouldBe(AbpSession.UserId.Value);

            var externalLoginInfo = await context.UserExternalLoginInfos.FirstOrDefaultAsync(info =>
                info.UserId == AbpSession.UserId.Value && info.LoginProvider == "Google");
            externalLoginInfo.ShouldNotBeNull();
            externalLoginInfo.EmailAddress.ShouldBe("google-merge-token@test.com");
        });

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            await _externalLoginLinkManager.MergeAndLinkExternalLoginAsync(
                AbpSession.UserId.Value,
                AbpSession.TenantId,
                mergeToken,
                "2mF9d8Ac!5"));

        exception.Message.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Reject_Merge_Token_Requested_By_Another_User()
    {
        var targetUserId = await CreateUserAndGetId("tokenowner", "2mF9d8Ac!5");
        var otherCurrentUserId = await CreateUserAndGetId("anothercurrentuser");

        UsingDbContext(context =>
        {
            context.UserLogins.Add(new UserLogin
            {
                LoginProvider = "Google",
                ProviderKey = "google-foreign-token-key",
                TenantId = AbpSession.TenantId,
                UserId = targetUserId
            });
            context.SaveChanges();
        });

        var mergeToken = await _externalLoginLinkManager.CreateMergeTokenAsync(
            AbpSession.UserId.Value,
            AbpSession.TenantId,
            "Google",
            "google-foreign-token-key",
            null);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            await _externalLoginLinkManager.MergeAndLinkExternalLoginAsync(
                otherCurrentUserId,
                AbpSession.TenantId,
                mergeToken,
                "2mF9d8Ac!5"));

        exception.Message.ShouldNotBeNullOrEmpty();

        await UsingDbContextAsync(async context =>
        {
            var login = await context.UserLogins.FirstOrDefaultAsync(ul =>
                ul.LoginProvider == "Google" && ul.ProviderKey == "google-foreign-token-key");
            login.ShouldNotBeNull();
            login.UserId.ShouldBe(targetUserId);
        });
    }

    [Fact]
    public async Task Should_Reject_Merge_Token_Requested_For_Another_Tenant()
    {
        AbpSession.TenantId.ShouldNotBeNull();

        var targetUserId = await CreateUserAndGetId("tokentenantowner", "2mF9d8Ac!5");
        UsingDbContext(context =>
        {
            context.UserLogins.Add(new UserLogin
            {
                LoginProvider = "Google",
                ProviderKey = "google-tenant-token-key",
                TenantId = AbpSession.TenantId,
                UserId = targetUserId
            });
            context.SaveChanges();
        });

        var mergeToken = await _externalLoginLinkManager.CreateMergeTokenAsync(
            AbpSession.UserId.Value,
            AbpSession.TenantId.Value + 1,
            "Google",
            "google-tenant-token-key",
            null);

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            await _externalLoginLinkManager.MergeAndLinkExternalLoginAsync(
                AbpSession.UserId.Value,
                AbpSession.TenantId,
                mergeToken,
                "2mF9d8Ac!5"));

        exception.Message.ShouldNotBeNullOrEmpty();

        await UsingDbContextAsync(async context =>
        {
            var login = await context.UserLogins.FirstOrDefaultAsync(ul =>
                ul.LoginProvider == "Google" && ul.ProviderKey == "google-tenant-token-key");
            login.ShouldNotBeNull();
            login.UserId.ShouldBe(targetUserId);
        });
    }

    [Fact]
    public async Task Should_Reject_Missing_Or_Expired_Merge_Token()
    {
        var targetUserId = await CreateUserAndGetId("missingtokenowner", "2mF9d8Ac!5");
        UsingDbContext(context =>
        {
            context.UserLogins.Add(new UserLogin
            {
                LoginProvider = "Google",
                ProviderKey = "google-missing-token-key",
                TenantId = AbpSession.TenantId,
                UserId = targetUserId
            });
            context.SaveChanges();
        });

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            await _externalLoginLinkManager.MergeAndLinkExternalLoginAsync(
                AbpSession.UserId.Value,
                AbpSession.TenantId,
                "missing-or-expired-merge-token",
                "2mF9d8Ac!5"));

        exception.Message.ShouldNotBeNullOrEmpty();

        await UsingDbContextAsync(async context =>
        {
            var login = await context.UserLogins.FirstOrDefaultAsync(ul =>
                ul.LoginProvider == "Google" && ul.ProviderKey == "google-missing-token-key");
            login.ShouldNotBeNull();
            login.UserId.ShouldBe(targetUserId);
        });
    }

    private async Task<long> CreateUserAndGetId(string userName, string password = "123qwE*")
    {
        var user = new CadentManagement.Authorization.Users.User
        {
            TenantId = AbpSession.TenantId,
            UserName = userName,
            Name = userName,
            Surname = "Test",
            EmailAddress = $"{userName}@test.com",
            IsActive = true,
            IsEmailConfirmed = true
        };

        var userManager = Resolve<CadentManagement.Authorization.Users.UserManager>();
        var result = await userManager.CreateAsync(user, password);
        result.Succeeded.ShouldBeTrue();

        return user.Id;
    }
}
