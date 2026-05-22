using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using Abp.OpenIddict.EntityFrameworkCore.Applications;
using CadentManagement.EntityFrameworkCore;

namespace CadentManagement.OpenIddict.Applications;

public class OpenIddictApplicationRepository : EfCoreOpenIddictApplicationRepository<CadentManagementDbContext>
{
    public OpenIddictApplicationRepository(
        IDbContextProvider<CadentManagementDbContext> dbContextProvider,
        IUnitOfWorkManager unitOfWorkManager) : base(dbContextProvider, unitOfWorkManager)
    {
    }
}

