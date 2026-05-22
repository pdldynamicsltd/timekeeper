using System.Threading.Tasks;

namespace CadentManagement.Authorization.Accounts;

public class ProxyTokenAuthControllerService : ProxyControllerBase
{
    public async Task SendTwoFactorAuthCode(long userId, string provider)
    {
        await ApiClient
            .PostAsync("api/" + GetEndpoint(nameof(SendTwoFactorAuthCode)), new { UserId = userId, Provider = provider });
    }

    public async Task AuthenticateWithQrCode(string connectionId, string sessionId)
    {
        await ApiClient
            .PostAsync("api/" + GetEndpoint(nameof(AuthenticateWithQrCode)), new { ConnectionId = connectionId, SessionId = sessionId });
    }
}

