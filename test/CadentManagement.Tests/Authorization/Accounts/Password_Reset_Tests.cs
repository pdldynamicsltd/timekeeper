using System;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Localization;
using Abp.Timing;
using Abp.UI;
using Abp.Zero.Configuration;
using Castle.MicroKernel.Registration;
using Microsoft.AspNetCore.Identity;
using CadentManagement.Authorization.Accounts;
using CadentManagement.Authorization.Accounts.Dto;
using CadentManagement.Authorization.Users;
using NSubstitute;
using Shouldly;
using Xunit;


namespace CadentManagement.Tests.Authorization.Accounts;

// ReSharper disable once InconsistentNaming
public class Password_Reset_Tests : AppTestBase
{
    private readonly IUserEmailer _fakeUserEmailer;

    public Password_Reset_Tests()
    {
        _fakeUserEmailer = Substitute.For<IUserEmailer>();
        _fakeUserEmailer
            .SendPasswordResetLinkAsync(Arg.Any<User>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);
        _fakeUserEmailer
            .SendPasswordChangedNotificationAsync(Arg.Any<User>())
            .Returns(Task.CompletedTask);

        LocalIocManager.IocContainer.Register(
            Component.For<IUserEmailer>().Instance(_fakeUserEmailer).IsDefault());
    }

    [Fact]
    public async Task Should_Reset_Password()
    {
        //Arrange
        var user = await GetCurrentUserAsync();
        string passResetCode = null;

        _fakeUserEmailer.SendPasswordResetLinkAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(callInfo =>
        {
            var calledUser = callInfo.Arg<User>();
            calledUser.EmailAddress.ShouldBe(user.EmailAddress);
            passResetCode = calledUser.PasswordResetCode;
            return Task.CompletedTask;
        });

        var accountAppService = Resolve<IAccountAppService>();

        //Act
        await accountAppService.SendPasswordResetCode(
            new SendPasswordResetCodeInput
            {
                EmailAddress = user.EmailAddress
            }
        );

        await accountAppService.ResetPassword(
            new ResetPasswordInput
            {
                Password = "New@Passw0rd",
                ResetCode = passResetCode,
                UserId = user.Id
            }
        );

        //Assert
        user = await GetCurrentUserAsync();
        LocalIocManager
            .Resolve<IPasswordHasher<User>>()
            .VerifyHashedPassword(user, user.Password, "New@Passw0rd")
            .ShouldBe(PasswordVerificationResult.Success);

        user.PasswordResetCode.ShouldBeNull();
        user.PasswordResetCodeExpireDate.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Send_Password_Changed_Notification_After_Reset()
    {
        //Arrange
        var user = await GetCurrentUserAsync();
        string passResetCode = null;

        _fakeUserEmailer.SendPasswordResetLinkAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(callInfo =>
        {
            passResetCode = callInfo.Arg<User>().PasswordResetCode;
            return Task.CompletedTask;
        });

        var accountAppService = Resolve<IAccountAppService>();

        //Act
        await accountAppService.SendPasswordResetCode(
            new SendPasswordResetCodeInput { EmailAddress = user.EmailAddress }
        );

        await accountAppService.ResetPassword(
            new ResetPasswordInput
            {
                Password = "New@Passw0rd",
                ResetCode = passResetCode,
                UserId = user.Id
            }
        );

        //Assert
        await _fakeUserEmailer.Received(1)
            .SendPasswordChangedNotificationAsync(Arg.Is<User>(u => u.Id == user.Id));
    }

    [Fact]
    public async Task Should_Not_Reset_Password_When_ResetCode_Is_Expired()
    {
        //Arrange
        var user = await GetCurrentUserAsync();
        string resetCode = null;

        _fakeUserEmailer.SendPasswordResetLinkAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(callInfo =>
        {
            resetCode = callInfo.Arg<User>().PasswordResetCode;
            return Task.CompletedTask;
        });

        var accountAppService = Resolve<IAccountAppService>();

        await accountAppService.SendPasswordResetCode(
            new SendPasswordResetCodeInput { EmailAddress = user.EmailAddress }
        );

        // Simulate server-side expiry by setting the expire date in the past
        UsingDbContext(context =>
        {
            var dbUser = context.Users.Find(user.Id);
            dbUser.PasswordResetCodeExpireDate = Clock.Now.AddDays(-1);
        });

        //Act & Assert
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
        {
            await accountAppService.ResetPassword(new ResetPasswordInput
            {
                UserId = user.Id,
                ResetCode = resetCode,
                Password = "123qwe"
            });
        });

        var localizationManager = Resolve<ILocalizationManager>();
        exception.Message.ShouldContain(
            localizationManager.GetString(
                CadentManagementConsts.LocalizationSourceName,
                "PasswordResetLinkExpired"));
    }

    [Fact]
    public async Task Should_Not_Reset_Password_With_Wrong_ResetCode()
    {
        //Arrange
        var user = await GetCurrentUserAsync();

        _fakeUserEmailer.SendPasswordResetLinkAsync(Arg.Any<User>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        var accountAppService = Resolve<IAccountAppService>();

        await accountAppService.SendPasswordResetCode(
            new SendPasswordResetCodeInput { EmailAddress = user.EmailAddress }
        );

        //Act & Assert
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
        {
            await accountAppService.ResetPassword(new ResetPasswordInput
            {
                UserId = user.Id,
                ResetCode = "WRONGCODE1",
                Password = "New@Passw0rd"
            });
        });

        var localizationManager = Resolve<ILocalizationManager>();
        exception.Message.ShouldContain(
            localizationManager.GetString(
                CadentManagementConsts.LocalizationSourceName,
                "InvalidPasswordResetCode"));

        // AccessFailedCount should be incremented
        user = await GetCurrentUserAsync();
        user.AccessFailedCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Should_Lockout_User_After_Multiple_Failed_Reset_Attempts()
    {
        //Arrange
        const int maxAttempts = 3;
        var settingManager = Resolve<ISettingManager>();

        // Explicitly configure lockout settings for deterministic behavior
        await settingManager.ChangeSettingForTenantAsync(
            AbpSession.TenantId.Value,
            AbpZeroSettingNames.UserManagement.UserLockOut.IsEnabled,
            "true"
        );
        await settingManager.ChangeSettingForTenantAsync(
            AbpSession.TenantId.Value,
            AbpZeroSettingNames.UserManagement.UserLockOut.MaxFailedAccessAttemptsBeforeLockout,
            maxAttempts.ToString()
        );

        var user = await GetCurrentUserAsync();

        _fakeUserEmailer.SendPasswordResetLinkAsync(Arg.Any<User>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        var accountAppService = Resolve<IAccountAppService>();

        await accountAppService.SendPasswordResetCode(
            new SendPasswordResetCodeInput { EmailAddress = user.EmailAddress }
        );

        //Act - attempt wrong code enough times to trigger lockout
        for (var i = 0; i < maxAttempts; i++)
        {
            await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            {
                await accountAppService.ResetPassword(new ResetPasswordInput
                {
                    UserId = user.Id,
                    ResetCode = "WRONGCODE1",
                    Password = "New@Passw0rd"
                });
            });
        }

        //Assert - user should now be locked out
        user = await GetCurrentUserAsync();
        user.LockoutEndDateUtc.ShouldNotBeNull();
        user.LockoutEndDateUtc.Value.ShouldBeGreaterThan(Clock.Now.ToUniversalTime());

        // Further attempts should fail with lockout message
        var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
        {
            await accountAppService.ResetPassword(new ResetPasswordInput
            {
                UserId = user.Id,
                ResetCode = "WRONGCODE1",
                Password = "New@Passw0rd"
            });
        });

        var localizationManager = Resolve<ILocalizationManager>();
        exception.Message.ShouldContain(
            localizationManager.GetString(
                CadentManagementConsts.LocalizationSourceName,
                "UserLockedOutMessage"));
    }

    [Fact]
    public async Task Should_Not_Reset_Password_Without_Reset_Code()
    {
        //Arrange - user with no reset code set
        var user = await GetCurrentUserAsync();
        var accountAppService = Resolve<IAccountAppService>();

        //Act & Assert
        await Assert.ThrowsAsync<UserFriendlyException>(async () =>
        {
            await accountAppService.ResetPassword(new ResetPasswordInput
            {
                UserId = user.Id,
                ResetCode = "ANYCODE123",
                Password = "New@Passw0rd"
            });
        });
    }
}
