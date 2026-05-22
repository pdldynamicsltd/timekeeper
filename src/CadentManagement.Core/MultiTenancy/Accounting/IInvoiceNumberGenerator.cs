using System.Threading.Tasks;
using Abp.Dependency;

namespace CadentManagement.MultiTenancy.Accounting;

public interface IInvoiceNumberGenerator : ITransientDependency
{
    Task<string> GetNewInvoiceNumber();
}

