using Abp.Mapperly;
using CadentManagement.TimeTracking.TimeEntries;
using CadentManagement.TimeTracking.TimeEntries.Dto;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Mappers;

[Mapper]
public partial class TimeEntryToTimeEntryDtoMapper : MapperBase<TimeEntry, TimeEntryDto>
{
    public override partial TimeEntryDto Map(TimeEntry source);

    public override partial void Map(TimeEntry source, TimeEntryDto destination);
}

[Mapper]
public partial class CreateOrEditTimeEntryDtoToTimeEntryMapper : MapperBase<CreateOrEditTimeEntryDto, TimeEntry>
{
    public override partial TimeEntry Map(CreateOrEditTimeEntryDto source);

    public override partial void Map(CreateOrEditTimeEntryDto source, TimeEntry destination);
}
