using System.Collections.Generic;
using System.Threading.Tasks;
using CadentManagement.Authorization.Users.Dto;
using CadentManagement.Dto;

namespace CadentManagement.Authorization.Users.Exporting;

public interface IUserListExcelExporter
{
    Task<FileDto> ExportToFile(List<UserListDto> userListDtos, List<string> selectedColumns);
}