using CadentManagement.Auditing.Dto;
using CadentManagement.Dto;
using CadentManagement.EntityChanges.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CadentManagement.Auditing.Exporting;

public interface IAuditLogListExcelExporter
{
    Task<FileDto> ExportToFile(List<AuditLogListDto> auditLogListDtos);

    Task<FileDto> ExportToFile(List<EntityChangeListDto> entityChangeListDtos);
}
