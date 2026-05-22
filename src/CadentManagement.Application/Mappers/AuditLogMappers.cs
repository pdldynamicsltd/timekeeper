using Abp.Auditing;
using Abp.Mapperly;
using CadentManagement.Auditing.Dto;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Mappers;

[Mapper]
public partial class AuditLogToAuditLogListDtoMapper : MapperBase<AuditLog, AuditLogListDto>
{
    public override partial AuditLogListDto Map(AuditLog source);

    public override partial void Map(AuditLog source, AuditLogListDto destination);
}
