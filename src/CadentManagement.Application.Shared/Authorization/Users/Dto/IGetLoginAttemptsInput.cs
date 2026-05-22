using Abp.Application.Services.Dto;

namespace CadentManagement.Authorization.Users.Dto;

public interface IGetLoginAttemptsInput : ISortedResultRequest
{
    string Filter { get; set; }
}

