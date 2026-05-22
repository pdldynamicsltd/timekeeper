using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CadentManagement.Chat.Dto;

namespace CadentManagement.Chat;

public interface IChatAppService : IApplicationService
{
    Task<GetUserChatFriendsWithSettingsOutput> GetUserChatFriendsWithSettings();

    Task<ListResultDto<ChatMessageDto>> GetUserChatMessages(GetUserChatMessagesInput input);

    Task MarkAllUnreadMessagesOfUserAsRead(MarkAllUnreadMessagesOfUserAsReadInput input);
}

