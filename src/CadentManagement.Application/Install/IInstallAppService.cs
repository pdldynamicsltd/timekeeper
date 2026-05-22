using System.Threading.Tasks;
using Abp.Application.Services;
using CadentManagement.Install.Dto;

namespace CadentManagement.Install;

public interface IInstallAppService : IApplicationService
{
    Task Setup(InstallDto input);

    AppSettingsJsonDto GetAppSettingsJson();

    CheckDatabaseOutput CheckDatabase();
}
