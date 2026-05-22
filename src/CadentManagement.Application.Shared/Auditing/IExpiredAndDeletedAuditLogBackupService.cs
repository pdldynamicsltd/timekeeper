using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Auditing;

namespace CadentManagement.Auditing;

public interface IExpiredAndDeletedAuditLogBackupService
{
    bool CanBackup();

    Task Backup(List<AuditLog> auditLogs);
}