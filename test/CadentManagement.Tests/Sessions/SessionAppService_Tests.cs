using System.Threading.Tasks;
using CadentManagement.Sessions;
using CadentManagement.Test.Base;
using Shouldly;
using Xunit;

namespace CadentManagement.Tests.Sessions;

// ReSharper disable once InconsistentNaming
public class SessionAppService_Tests : AppTestBase
{
    private readonly ISessionAppService _sessionAppService;

    public SessionAppService_Tests()
    {
        _sessionAppService = Resolve<ISessionAppService>();
    }

    [MultiTenantFact]
    public async Task Should_Get_Current_User_When_Logged_In_As_Host()
    {
        //Arrange
        LoginAsHostAdmin();

        //Act
        var output = await _sessionAppService.GetCurrentLoginInformations();

        //Assert
        var currentUser = await GetCurrentUserAsync();
        output.User.ShouldNotBe(null);
        output.User.Name.ShouldBe(currentUser.Name);
        output.User.Surname.ShouldBe(currentUser.Surname);

        output.Tenant.ShouldBe(null);
    }

    [Fact]
    public async Task Should_Get_Current_User_And_Tenant_When_Logged_In_As_Tenant()
    {
        //Act
        var output = await _sessionAppService.GetCurrentLoginInformations();

        //Assert
        var currentUser = await GetCurrentUserAsync();
        var currentTenant = await GetCurrentTenantAsync();

        output.User.ShouldNotBe(null);
        output.User.Name.ShouldBe(currentUser.Name);

        output.Tenant.ShouldNotBe(null);
        output.Tenant.Name.ShouldBe(currentTenant.Name);

        output.Application.Version.ShouldBe(AppVersionHelper.Version);
        output.Application.ReleaseDate.ShouldBe(AppVersionHelper.ReleaseDate);
    }

    [Fact]
    public async Task Should_Map_SubscribableEdition_Properties_For_Tenant()
    {
        //Act
        var output = await _sessionAppService.GetCurrentLoginInformations();

        //Assert
        output.Tenant.ShouldNotBeNull();
        output.Tenant.Edition.ShouldNotBeNull();

        output.Tenant.Edition.IsFree.ShouldBeTrue();
        output.Tenant.Edition.MonthlyPrice.ShouldBeNull();
        output.Tenant.Edition.AnnualPrice.ShouldBeNull();
    }
}
