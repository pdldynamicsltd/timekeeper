using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CadentManagement.TimeTracking.Dto;
using CadentManagement.TimeTracking.TimeEntries.Dto;

namespace CadentManagement.TimeTracking.TimeEntries;

public interface ITimeEntryAppService : IApplicationService
{
    Task<GetTimeEntryForEditOutput> GetForEditAsync(NullableIdDto<int> input);

    Task<int> CreateAsync(CreateOrEditTimeEntryDto input);

    Task UpdateAsync(CreateOrEditTimeEntryDto input);

    Task DeleteAsync(EntityDto<int> input);

    Task<PagedResultDto<TimeEntryDto>> GetTimeEntriesAsync(GetTimeEntriesInput input);

    Task<List<SchedulerEntryDto>> GetSchedulerEntriesAsync(GetSchedulerEntriesInput input);

    Task<ImportCsvResultDto> ImportTimeEntriesFromCsvAsync(ImportTimeEntriesCsvInput input);
}
