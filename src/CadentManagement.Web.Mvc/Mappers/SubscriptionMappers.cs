using Abp.Mapperly;
using CadentManagement.MultiTenancy.Payments.Dto;
using CadentManagement.Web.Areas.App.Models.SubscriptionManagement;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Web.Mappers;

[Mapper]
public partial class SubscriptionPaymentProductDtoToShowDetailModalViewModelMapper : MapperBase<SubscriptionPaymentProductDto, ShowDetailModalViewModel>
{
    public override partial ShowDetailModalViewModel Map(SubscriptionPaymentProductDto source);
    public override partial void Map(SubscriptionPaymentProductDto source, ShowDetailModalViewModel destination);
}
