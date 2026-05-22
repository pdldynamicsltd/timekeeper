using System.Threading.Tasks;
using Abp.Application.Services;
using CadentManagement.Configuration.Host.Dto;

namespace CadentManagement.Configuration.Host;

public interface IHostSettingsAppService : IApplicationService
{
    Task<HostSettingsEditDto> GetAllSettings();

    Task UpdateAllSettings(HostSettingsEditDto input);

    Task SendTestEmail(SendTestEmailInput input);
}

