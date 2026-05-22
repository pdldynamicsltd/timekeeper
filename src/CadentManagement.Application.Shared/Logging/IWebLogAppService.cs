using Abp.Application.Services;
using CadentManagement.Dto;
using CadentManagement.Logging.Dto;

namespace CadentManagement.Logging;

public interface IWebLogAppService : IApplicationService
{
    GetLatestWebLogsOutput GetLatestWebLogs();

    FileDto DownloadWebLogs();
}

