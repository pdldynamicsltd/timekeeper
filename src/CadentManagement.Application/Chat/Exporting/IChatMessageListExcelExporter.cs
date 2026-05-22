using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using CadentManagement.Chat.Dto;
using CadentManagement.Dto;

namespace CadentManagement.Chat.Exporting;

public interface IChatMessageListExcelExporter
{
    Task<FileDto> ExportToFile(UserIdentifier user, List<ChatMessageExportDto> messages);
}