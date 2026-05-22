using Abp.Mapperly;
using System.Collections.Generic;
using System.Linq;
using CadentManagement.MultiTenancy.Payments;
using CadentManagement.MultiTenancy.Payments.Dto;
using CadentManagement.Sessions.Dto;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Mappers;

[Mapper]
public partial class SubscriptionPaymentToSubscriptionPaymentDtoMapper : MapperBase<SubscriptionPayment, SubscriptionPaymentDto>
{
    private static readonly SubscriptionPaymentProductToSubscriptionPaymentProductDtoMapper _productMapper = new();

    public override SubscriptionPaymentDto Map(SubscriptionPayment source)
    {
        var dto = MapInternal(source);
        dto.TotalAmount = source.GetTotalAmount();
        dto.SubscriptionPaymentProducts = source.SubscriptionPaymentProducts?
            .Select(p => _productMapper.Map(p))
            .ToList() ?? new List<SubscriptionPaymentProductDto>();
        return dto;
    }

    public override void Map(SubscriptionPayment source, SubscriptionPaymentDto destination)
    {
        MapInternal(source, destination);
        destination.TotalAmount = source.GetTotalAmount();
        destination.SubscriptionPaymentProducts = source.SubscriptionPaymentProducts?
            .Select(p => _productMapper.Map(p))
            .ToList() ?? new List<SubscriptionPaymentProductDto>();
    }

    [MapperIgnoreTarget(nameof(SubscriptionPaymentDto.TotalAmount))]
    private partial SubscriptionPaymentDto MapInternal(SubscriptionPayment source);

    [MapperIgnoreTarget(nameof(SubscriptionPaymentDto.TotalAmount))]
    private partial void MapInternal(SubscriptionPayment source, SubscriptionPaymentDto destination);
}

[Mapper]
public partial class SubscriptionPaymentDtoToSubscriptionPaymentMapper : MapperBase<SubscriptionPaymentDto, SubscriptionPayment>
{
    public override partial SubscriptionPayment Map(SubscriptionPaymentDto source);

    public override partial void Map(SubscriptionPaymentDto source, SubscriptionPayment destination);
}

[Mapper]
public partial class SubscriptionPaymentProductToSubscriptionPaymentProductDtoMapper : MapperBase<SubscriptionPaymentProduct, SubscriptionPaymentProductDto>
{
    public override partial SubscriptionPaymentProductDto Map(SubscriptionPaymentProduct source);

    public override partial void Map(SubscriptionPaymentProduct source, SubscriptionPaymentProductDto destination);
}

[Mapper]
public partial class SubscriptionPaymentProductDtoToSubscriptionPaymentProductMapper : MapperBase<SubscriptionPaymentProductDto, SubscriptionPaymentProduct>
{
    public override partial SubscriptionPaymentProduct Map(SubscriptionPaymentProductDto source);

    public override partial void Map(SubscriptionPaymentProductDto source, SubscriptionPaymentProduct destination);
}

[Mapper]
public partial class SubscriptionPaymentToSubscriptionPaymentListDtoMapper : MapperBase<SubscriptionPayment, SubscriptionPaymentListDto>
{
    private static readonly SubscriptionPaymentProductToSubscriptionPaymentProductDtoMapper _productMapper = new();

    public override SubscriptionPaymentListDto Map(SubscriptionPayment source)
    {
        var dto = new SubscriptionPaymentListDto
        {
            Id = source.Id,
            CreationTime = source.CreationTime,
            CreatorUserId = source.CreatorUserId,
            LastModificationTime = source.LastModificationTime,
            LastModifierUserId = source.LastModifierUserId,
            Gateway = source.Gateway.ToString(),
            DayCount = source.DayCount,
            PaymentPeriodType = source.PaymentPeriodType.ToString(),
            ExternalPaymentId = source.ExternalPaymentId,
            Status = source.Status.ToString(),
            TenantId = source.TenantId,
            InvoiceNo = source.InvoiceNo,
            SubscriptionPaymentProducts = source.SubscriptionPaymentProducts?
                .Select(p => _productMapper.Map(p))
                .ToList() ?? new List<SubscriptionPaymentProductDto>()
        };
        return dto;
    }

    public override void Map(SubscriptionPayment source, SubscriptionPaymentListDto destination)
    {
        destination.Id = source.Id;
        destination.CreationTime = source.CreationTime;
        destination.CreatorUserId = source.CreatorUserId;
        destination.LastModificationTime = source.LastModificationTime;
        destination.LastModifierUserId = source.LastModifierUserId;
        destination.Gateway = source.Gateway.ToString();
        destination.DayCount = source.DayCount;
        destination.PaymentPeriodType = source.PaymentPeriodType.ToString();
        destination.ExternalPaymentId = source.ExternalPaymentId;
        destination.Status = source.Status.ToString();
        destination.TenantId = source.TenantId;
        destination.InvoiceNo = source.InvoiceNo;
        destination.SubscriptionPaymentProducts = source.SubscriptionPaymentProducts?
            .Select(p => _productMapper.Map(p))
            .ToList() ?? new List<SubscriptionPaymentProductDto>();
    }
}

[Mapper]
public partial class SubscriptionPaymentListDtoToSubscriptionPaymentMapper : MapperBase<SubscriptionPaymentListDto, SubscriptionPayment>
{
    [MapperIgnoreTarget(nameof(SubscriptionPayment.ErrorUrl))]
    public override partial SubscriptionPayment Map(SubscriptionPaymentListDto source);

    [MapperIgnoreTarget(nameof(SubscriptionPayment.ErrorUrl))]
    public override partial void Map(SubscriptionPaymentListDto source, SubscriptionPayment destination);
}

[Mapper]
public partial class SubscriptionPaymentToSubscriptionPaymentInfoDtoMapper : MapperBase<SubscriptionPayment, SubscriptionPaymentInfoDto>
{
    [MapperIgnoreSource(nameof(SubscriptionPayment.ErrorUrl))]
    public override partial SubscriptionPaymentInfoDto Map(SubscriptionPayment source);

    [MapperIgnoreSource(nameof(SubscriptionPayment.ErrorUrl))]
    public override partial void Map(SubscriptionPayment source, SubscriptionPaymentInfoDto destination);
}
