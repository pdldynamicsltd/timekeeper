using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using CadentManagement.MultiTenancy.Accounting.Dto;

namespace CadentManagement.MultiTenancy.Accounting;

public interface IInvoiceAppService
{
    Task<InvoiceDto> GetInvoiceInfo(EntityDto<long> input);

    Task CreateInvoice(CreateInvoiceDto input);
}
