using System.Threading.Tasks;

namespace CadentManagement.Net.Sms;

public interface ISmsSender
{
    Task SendAsync(string number, string message);
}

