using CadentManagement.EntityFrameworkCore;

namespace CadentManagement.Test.Base.TestData;

public class TestDataBuilder
{
    private readonly CadentManagementDbContext _context;
    private readonly int _tenantId;

    public TestDataBuilder(CadentManagementDbContext context, int tenantId)
    {
        _context = context;
        _tenantId = tenantId;
    }

    public void Create()
    {
        new TestOrganizationUnitsBuilder(_context, _tenantId).Create();
        new TestSubscriptionPaymentBuilder(_context, _tenantId).Create();
        new TestEditionsBuilder(_context).Create();

        _context.SaveChanges();
    }
}
