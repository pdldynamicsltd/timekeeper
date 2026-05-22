using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CadentManagement.EntityChanges.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CadentManagement.EntityChanges;

public interface IEntityChangeAppService : IApplicationService
{
    Task<ListResultDto<EntityAndPropertyChangeListDto>> GetEntityChangesByEntity(GetEntityChangesByEntityInput input);
}

