using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.UI;
using Castle.MicroKernel.Registration;
using Microsoft.EntityFrameworkCore;
using CadentManagement.Authorization;
using CadentManagement.Authorization.Users.ExternalLoginLink;
using CadentManagement.Authorization.Users.ExternalLoginLink.Dto;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CadentManagement.Tests.Authorization.Users;

// ReSharper disable once InconsistentNaming
public class ExternalLoginLinkAppService_Tests : AppTestBase
{
    private readonly IExternalLoginLinkAppService _externalLoginLinkAppService;
    private readonly IExternalLoginProviderManager _externalLoginProviderManager;

    public ExternalLoginLinkAppService_Tests()
    {
        _externalLoginProviderManager = Substitute.For<IExternalLoginProviderManager>();
        _externalLoginProviderManager.GetAvailableProviders().Returns(new List<AvailableExternalLoginProvider>
        {
            new() { Name = "Google", ClientId = "google-client" },
            new() { Name = "Facebook", ClientId = "facebook-client" },
            new() { Name = "Microsoft", ClientId = "microsoft-client" }
        });
        _externalLoginProviderManager.GetProviderKeyAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Task.FromResult(GetProviderKey(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1))));
        _externalLoginProviderManager.GetUserInfoAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(callInfo => Task.FromResult(new ExternalLoginUserInfoDto
            {
                ProviderKey = GetProviderKey(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1)),
                EmailAddress = GetProviderEmailAddress(callInfo.ArgAt<string>(0), callInfo.ArgAt<string>(1))
            }));

        LocalIocManager.IocContainer.Register(
            Component.For<IExternalLoginProviderManager>().Instance(_externalLoginProviderManager).IsDefault());

        _externalLoginLinkAppService = Resolve<IExternalLoginLinkAppService>();
    }

    [Fact]
    public async Task Should_Link_External_Login()
    {
        var result = await _externalLoginLinkAppService.LinkExternalLogin(new LinkExternalLoginInput
        {
            AuthProvider = "Google",
            ProviderKey = "google-user-123",
            ProviderAccessCode = "google-user-123-access-code"
        });

        result.Success.ShouldBeTrue();
        result.ProviderAlreadyLinkedToAnotherUser.ShouldBeFalse();

        await UsingDbContextAsync(async context =>
        {
            var login = await context.UserLogins.FirstOrDefaultAsync(ul =>
                ul.LoginProvider == "Google" && ul.ProviderKey == "google-user-123");
            login.ShouldNotBeNull();
            login.UserId.ShouldBe(AbpSession.UserId.Value);

            var externalLoginInfo = await context.UserExternalLoginInfos.FirstOrDefaultAsync(info =>
                info.UserId == AbpSession.UserId.Value && info.LoginProvider == "Google");
            externalLoginInfo.ShouldNotBeNull();
            externalLoginInfo.EmailAddress.ShouldBe("google-user-123@test.com");
        });
    }

    [Fact]
    public async Task Should_Reject_Server_Side_Validation_Sentinel_On_Public_App_Service()
    {
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            await _externalLoginLinkAppService.LinkExternalLogin(new LinkExternalLoginInput
            {
                AuthProvider = "Google",
                ProviderKey = "google-user-123",
                ProviderAccessCode = "server-side-validated"
            }));

        exception.Message.ShouldNotBeNullOrEmpty();
        await _externalLoginProviderManager.Received(1).GetUserInfoAsync("Google", "server-side-validated");
    }

    [Fact]
    public async Task Should_Not_Link_Same_Provider_Twice()
    {
        await _externalLoginLinkAppService.LinkExternalLogin(new LinkExternalLoginInput
        {
            AuthProvider = "Google",
            ProviderKey = "google-user-123",
            ProviderAccessCode = "google-user-123-access-code"
        });

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            await _externalLoginLinkAppService.LinkExternalLogin(new LinkExternalLoginInput
            {
                AuthProvider = "Google",
                ProviderKey = "google-user-456",
                ProviderAccessCode = "google-user-456-access-code"
            }));

        exception.Message.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Return_Merge_Option_When_Provider_Linked_To_Another_User()
    {
        var otherUserId = await CreateUserAndGetId("otheruser");
        UsingDbContext(context =>
        {
            context.UserLogins.Add(new UserLogin
            {
                LoginProvider = "Google",
                ProviderKey = "google-other-user-key",
                TenantId = AbpSession.TenantId,
                UserId = otherUserId
            });
            context.SaveChanges();
        });

        var result = await _externalLoginLinkAppService.LinkExternalLogin(new LinkExternalLoginInput
        {
            AuthProvider = "Google",
            ProviderKey = "google-other-user-key",
            ProviderAccessCode = "google-other-user-access-code"
        });

        result.Success.ShouldBeFalse();
        result.ProviderAlreadyLinkedToAnotherUser.ShouldBeTrue();
        result.CanMerge.ShouldBeTrue();
        result.ExistingUserEmail.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Mask_Malformed_Email_When_Provider_Linked_To_Another_User()
    {
        var otherUserId = await CreateUserAndGetId("malformedemailuser");
        UsingDbContext(context =>
        {
            var otherUser = context.Users.First(u => u.Id == otherUserId);
            otherUser.EmailAddress = "malformed-email";

            context.UserLogins.Add(new UserLogin
            {
                LoginProvider = "Google",
                ProviderKey = "google-malformed-email-key",
                TenantId = AbpSession.TenantId,
                UserId = otherUserId
            });

            context.SaveChanges();
        });

        var result = await _externalLoginLinkAppService.LinkExternalLogin(new LinkExternalLoginInput
        {
            AuthProvider = "Google",
            ProviderKey = "google-malformed-email-key",
            ProviderAccessCode = "google-malformed-email-access-code"
        });

        result.Success.ShouldBeFalse();
        result.ExistingUserEmail.ShouldBe("***");
    }

    [Fact]
    public async Task Should_Unlink_External_Login()
    {
        await _externalLoginLinkAppService.LinkExternalLogin(new LinkExternalLoginInput
        {
            AuthProvider = "Google",
            ProviderKey = "google-user-456",
            ProviderAccessCode = "google-user-456-access-code"
        });

        await _externalLoginLinkAppService.LinkExternalLogin(new LinkExternalLoginInput
        {
            AuthProvider = "Facebook",
            ProviderKey = "fb-user-123",
            ProviderAccessCode = "facebook-user-123-access-code"
        });

        var result = await _externalLoginLinkAppService.UnlinkExternalLogin(new UnlinkExternalLoginInput
        {
            AuthProvider = "Facebook"
        });

        result.Success.ShouldBeTrue();
        result.RequiresPasswordSetup.ShouldBeFalse();

        await UsingDbContextAsync(async context =>
        {
            var login = await context.UserLogins.FirstOrDefaultAsync(ul =>
                ul.LoginProvider == "Facebook" && ul.UserId == AbpSession.UserId.Value);
            login.ShouldBeNull();

            var externalLoginInfo = await context.UserExternalLoginInfos.FirstOrDefaultAsync(info =>
                info.LoginProvider == "Facebook" && info.UserId == AbpSession.UserId.Value);
            externalLoginInfo.ShouldBeNull();
        });
    }

    [Fact]
    public async Task Should_Block_Unlink_Last_External_Login_For_External_Login_Only_User()
    {
        UsingDbContext(context =>
        {
            context.UserClaims.Add(new UserClaim
            {
                UserId = AbpSession.UserId.Value,
                TenantId = AbpSession.TenantId,
                ClaimType = ExternalLoginConsts.ExternalLoginOnlyClaimType,
                ClaimValue = "true"
            });
        });

        await _externalLoginLinkAppService.LinkExternalLogin(new LinkExternalLoginInput
        {
            AuthProvider = "Facebook",
            ProviderKey = "fb-user-last",
            ProviderAccessCode = "facebook-user-last-access-code"
        });

        var result = await _externalLoginLinkAppService.UnlinkExternalLogin(new UnlinkExternalLoginInput
        {
            AuthProvider = "Facebook"
        });

        result.Success.ShouldBeFalse();
        result.RequiresPasswordSetup.ShouldBeTrue();

        await UsingDbContextAsync(async context =>
        {
            var login = await context.UserLogins.FirstOrDefaultAsync(ul =>
                ul.LoginProvider == "Facebook" && ul.UserId == AbpSession.UserId.Value);
            login.ShouldNotBeNull();
        });
    }

    [Fact]
    public async Task Should_Not_Unlink_When_Not_Linked()
    {
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            await _externalLoginLinkAppService.UnlinkExternalLogin(new UnlinkExternalLoginInput
            {
                AuthProvider = "Google"
            }));

        exception.Message.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Merge_And_Link_External_Login()
    {
        var otherUserId = await CreateUserAndGetId("mergeuser", "2mF9d8Ac!5");
        UsingDbContext(context =>
        {
            context.UserLogins.Add(new UserLogin
            {
                LoginProvider = "Microsoft",
                ProviderKey = "ms-merge-key",
                TenantId = AbpSession.TenantId,
                UserId = otherUserId
            });
            context.SaveChanges();
        });

        await _externalLoginLinkAppService.MergeAndLinkExternalLogin(new MergeExternalLoginInput
        {
            AuthProvider = "Microsoft",
            ProviderKey = "ms-merge-key",
            ProviderAccessCode = "microsoft-merge-access-code",
            TargetUserPassword = "2mF9d8Ac!5"
        });

        await UsingDbContextAsync(async context =>
        {
            var login = await context.UserLogins.FirstOrDefaultAsync(ul =>
                ul.LoginProvider == "Microsoft" && ul.ProviderKey == "ms-merge-key");
            login.ShouldNotBeNull();
            login.UserId.ShouldBe(AbpSession.UserId.Value);

            var externalLoginInfo = await context.UserExternalLoginInfos.FirstOrDefaultAsync(info =>
                info.LoginProvider == "Microsoft" && info.UserId == AbpSession.UserId.Value);
            externalLoginInfo.ShouldNotBeNull();
            externalLoginInfo.EmailAddress.ShouldBe("ms-merge@test.com");
        });
    }

    [Fact]
    public async Task Should_Not_Merge_With_Wrong_Password()
    {
        var otherUserId = await CreateUserAndGetId("wrongpwuser", "2mF9d8Ac!5");
        UsingDbContext(context =>
        {
            context.UserLogins.Add(new UserLogin
            {
                LoginProvider = "Google",
                ProviderKey = "google-wrong-pw-key",
                TenantId = AbpSession.TenantId,
                UserId = otherUserId
            });
            context.SaveChanges();
        });

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            await _externalLoginLinkAppService.MergeAndLinkExternalLogin(new MergeExternalLoginInput
            {
                AuthProvider = "Google",
                ProviderKey = "google-wrong-pw-key",
                ProviderAccessCode = "google-wrong-pw-access-code",
                TargetUserPassword = "WrongPassword123!"
            }));

        exception.Message.ShouldNotBeNullOrEmpty();

        await UsingDbContextAsync(async context =>
        {
            var login = await context.UserLogins.FirstOrDefaultAsync(ul =>
                ul.LoginProvider == "Google" && ul.ProviderKey == "google-wrong-pw-key");
            login.ShouldNotBeNull();
            login.UserId.ShouldBe(otherUserId);
        });
    }

    [Fact]
    public async Task Should_Get_External_Logins()
    {
        await _externalLoginLinkAppService.LinkExternalLogin(new LinkExternalLoginInput
        {
            AuthProvider = "Google",
            ProviderKey = "google-get-test",
            ProviderAccessCode = "google-get-access-code"
        });

        await _externalLoginLinkAppService.LinkExternalLogin(new LinkExternalLoginInput
        {
            AuthProvider = "Facebook",
            ProviderKey = "fb-get-test",
            ProviderAccessCode = "facebook-get-access-code"
        });

        var result = await _externalLoginLinkAppService.GetExternalLogins();

        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThanOrEqualTo(2);
        result.ShouldContain(p => p.Name == "Google" && p.IsLinked && p.EmailAddress == "google-get@test.com");
        result.ShouldContain(p => p.Name == "Facebook" && p.IsLinked && p.EmailAddress == "facebook-get@test.com");
    }

    private static string GetProviderKey(string providerName, string accessCode)
    {
        return (providerName, accessCode) switch
        {
            ("Google", "google-user-123-access-code") => "google-user-123",
            ("Google", "google-user-456-access-code") => "google-user-456",
            ("Google", "google-other-user-access-code") => "google-other-user-key",
            ("Google", "google-malformed-email-access-code") => "google-malformed-email-key",
            ("Google", "google-wrong-pw-access-code") => "google-wrong-pw-key",
            ("Google", "google-get-access-code") => "google-get-test",
            ("Facebook", "facebook-user-123-access-code") => "fb-user-123",
            ("Facebook", "facebook-user-last-access-code") => "fb-user-last",
            ("Facebook", "facebook-get-access-code") => "fb-get-test",
            ("Microsoft", "microsoft-merge-access-code") => "ms-merge-key",
            _ => null
        };
    }

    private static string GetProviderEmailAddress(string providerName, string accessCode)
    {
        return (providerName, accessCode) switch
        {
            ("Google", "google-user-123-access-code") => "google-user-123@test.com",
            ("Google", "google-user-456-access-code") => "google-user-456@test.com",
            ("Google", "google-other-user-access-code") => "google-other-user@test.com",
            ("Google", "google-malformed-email-access-code") => "malformed-email@test.com",
            ("Google", "google-wrong-pw-access-code") => "google-wrong-pw@test.com",
            ("Google", "google-get-access-code") => "google-get@test.com",
            ("Facebook", "facebook-user-123-access-code") => "facebook-user-123@test.com",
            ("Facebook", "facebook-user-last-access-code") => "facebook-user-last@test.com",
            ("Facebook", "facebook-get-access-code") => "facebook-get@test.com",
            ("Microsoft", "microsoft-merge-access-code") => "ms-merge@test.com",
            _ => null
        };
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

        await UsingDbContextAsync(async context =>
        {
            var created = await context.Users.FirstOrDefaultAsync(u =>
                u.UserName == userName && u.TenantId == AbpSession.TenantId);
            created.ShouldNotBeNull();
        });

        return user.Id;
    }
}
