using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using CadentManagement.Dto;

namespace CadentManagement.Gdpr;

public interface IUserCollectedDataProvider
{
    Task<List<FileDto>> GetFiles(UserIdentifier user);
}
