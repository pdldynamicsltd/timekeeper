using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using CadentManagement.RateLimiting;
using CadentManagement.RateLimiting.Dto;
using CadentManagement.Test.Base;
using Shouldly;
using Xunit;

namespace CadentManagement.Tests.RateLimiting;

// ReSharper disable once InconsistentNaming
public class RateLimitPolicyAppService_Tests : AppTestBase
{
    private readonly IRateLimitPolicyAppService _rateLimitPolicyAppService;

    public RateLimitPolicyAppService_Tests()
    {
        _rateLimitPolicyAppService = Resolve<IRateLimitPolicyAppService>();
        LoginAsHostAdmin();
    }

    [Fact]
    public async Task Should_Get_IsEnabled_Default_False()
    {
        // Act
        var isEnabled = await _rateLimitPolicyAppService.GetIsEnabled();

        // Assert
        isEnabled.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Set_IsEnabled()
    {
        // Act
        await _rateLimitPolicyAppService.SetIsEnabled(true);
        var isEnabled = await _rateLimitPolicyAppService.GetIsEnabled();

        // Assert
        isEnabled.ShouldBeTrue();

        // Cleanup
        await _rateLimitPolicyAppService.SetIsEnabled(false);
    }

    [Fact]
    public async Task Should_Create_Policy()
    {
        // Arrange
        var input = CreateTestPolicyInput();

        // Act
        await _rateLimitPolicyAppService.CreateOrEdit(input);

        // Assert
        var policies = await _rateLimitPolicyAppService.GetPolicies(new GetRateLimitPoliciesInput());
        policies.TotalCount.ShouldBe(1);
        policies.Items.First().Name.ShouldBe("Test Policy");
    }

    [Fact]
    public async Task Should_Update_Policy()
    {
        // Arrange
        var input = CreateTestPolicyInput();
        await _rateLimitPolicyAppService.CreateOrEdit(input);

        var policies = await _rateLimitPolicyAppService.GetPolicies(new GetRateLimitPoliciesInput());
        var policyId = policies.Items.First().Id;

        // Act
        var editOutput = await _rateLimitPolicyAppService.GetPolicyForEdit(new NullableIdDto { Id = policyId });
        editOutput.RateLimitPolicy.Name = "Updated Policy";
        editOutput.RateLimitPolicy.PermitLimit = 200;
        await _rateLimitPolicyAppService.CreateOrEdit(editOutput.RateLimitPolicy);

        // Assert
        var updatedPolicies = await _rateLimitPolicyAppService.GetPolicies(new GetRateLimitPoliciesInput());
        updatedPolicies.Items.First().Name.ShouldBe("Updated Policy");
        updatedPolicies.Items.First().PermitLimit.ShouldBe(200);
    }

    [Fact]
    public async Task Should_Delete_Policy()
    {
        // Arrange
        var input = CreateTestPolicyInput();
        await _rateLimitPolicyAppService.CreateOrEdit(input);

        var policies = await _rateLimitPolicyAppService.GetPolicies(new GetRateLimitPoliciesInput());
        var policyId = policies.Items.First().Id;

        // Act
        await _rateLimitPolicyAppService.Delete(new EntityDto(policyId));

        // Assert
        var remainingPolicies = await _rateLimitPolicyAppService.GetPolicies(new GetRateLimitPoliciesInput());
        remainingPolicies.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Should_Get_Policy_For_Edit()
    {
        // Arrange
        var input = CreateTestPolicyInput();
        await _rateLimitPolicyAppService.CreateOrEdit(input);

        var policies = await _rateLimitPolicyAppService.GetPolicies(new GetRateLimitPoliciesInput());
        var policyId = policies.Items.First().Id;

        // Act
        var editOutput = await _rateLimitPolicyAppService.GetPolicyForEdit(new NullableIdDto { Id = policyId });

        // Assert
        editOutput.RateLimitPolicy.ShouldNotBeNull();
        editOutput.RateLimitPolicy.Name.ShouldBe("Test Policy");
        editOutput.RateLimitPolicy.Algorithm.ShouldBe(RateLimitAlgorithm.FixedWindow);
        editOutput.RateLimitPolicy.PermitLimit.ShouldBe(100);
    }

    [Fact]
    public async Task Should_Get_Policy_For_Create_With_Defaults()
    {
        // Act
        var editOutput = await _rateLimitPolicyAppService.GetPolicyForEdit(new NullableIdDto());

        // Assert
        editOutput.RateLimitPolicy.ShouldNotBeNull();
        editOutput.RateLimitPolicy.Id.ShouldBeNull();
        editOutput.RateLimitPolicy.IsEnabled.ShouldBeTrue();
        editOutput.RateLimitPolicy.PermitLimit.ShouldBe(100);
        editOutput.RateLimitPolicy.WindowInSeconds.ShouldBe(60);
        editOutput.Algorithms.ShouldNotBeNull();
        editOutput.Algorithms.Count.ShouldBe(4);
        editOutput.PartitionTypes.ShouldNotBeNull();
        editOutput.PartitionTypes.Count.ShouldBe(3);
    }

    [Fact]
    public async Task Should_Toggle_Policy_Enabled()
    {
        // Arrange
        var input = CreateTestPolicyInput();
        input.IsEnabled = true;
        await _rateLimitPolicyAppService.CreateOrEdit(input);

        var policies = await _rateLimitPolicyAppService.GetPolicies(new GetRateLimitPoliciesInput());
        var policyId = policies.Items.First().Id;
        policies.Items.First().IsEnabled.ShouldBeTrue();

        // Act
        await _rateLimitPolicyAppService.TogglePolicyEnabled(new EntityDto(policyId));

        // Assert
        var updatedPolicies = await _rateLimitPolicyAppService.GetPolicies(new GetRateLimitPoliciesInput());
        updatedPolicies.Items.First().IsEnabled.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Get_Policies_With_Filter()
    {
        // Arrange
        await _rateLimitPolicyAppService.CreateOrEdit(CreateTestPolicyInput("Policy Alpha"));
        await _rateLimitPolicyAppService.CreateOrEdit(CreateTestPolicyInput("Policy Beta"));
        await _rateLimitPolicyAppService.CreateOrEdit(CreateTestPolicyInput("Other Policy"));

        // Act
        var result = await _rateLimitPolicyAppService.GetPolicies(new GetRateLimitPoliciesInput
        {
            Filter = "Alpha"
        });

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items.First().Name.ShouldBe("Policy Alpha");
    }

    [Fact]
    public async Task Should_Get_Policies_With_Algorithm_Filter()
    {
        // Arrange
        var fixedPolicy = CreateTestPolicyInput("Fixed Policy");
        fixedPolicy.Algorithm = RateLimitAlgorithm.FixedWindow;
        await _rateLimitPolicyAppService.CreateOrEdit(fixedPolicy);

        var slidingPolicy = CreateTestPolicyInput("Sliding Policy");
        slidingPolicy.Algorithm = RateLimitAlgorithm.SlidingWindow;
        slidingPolicy.SegmentsPerWindow = 3;
        await _rateLimitPolicyAppService.CreateOrEdit(slidingPolicy);

        // Act
        var result = await _rateLimitPolicyAppService.GetPolicies(new GetRateLimitPoliciesInput
        {
            Algorithm = RateLimitAlgorithm.SlidingWindow
        });

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items.First().Name.ShouldBe("Sliding Policy");
    }

    [Fact]
    public async Task Should_Get_Policies_With_Paging()
    {
        // Arrange
        for (var i = 1; i <= 5; i++)
        {
            await _rateLimitPolicyAppService.CreateOrEdit(CreateTestPolicyInput($"Policy {i}"));
        }

        // Act
        var result = await _rateLimitPolicyAppService.GetPolicies(new GetRateLimitPoliciesInput
        {
            MaxResultCount = 2,
            SkipCount = 0
        });

        // Assert
        result.TotalCount.ShouldBe(5);
        result.Items.Count.ShouldBe(2);
    }

    private static CreateOrEditRateLimitPolicyDto CreateTestPolicyInput(string name = "Test Policy")
    {
        return new CreateOrEditRateLimitPolicyDto
        {
            Name = name,
            IsEnabled = false,
            Algorithm = RateLimitAlgorithm.FixedWindow,
            PartitionType = RateLimitPartitionType.ByClientIp,
            IsGlobal = true,
            PermitLimit = 100,
            WindowInSeconds = 60,
            QueueLimit = 0,
            HttpStatusCode = 429
        };
    }
}
