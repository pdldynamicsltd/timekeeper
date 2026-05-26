using System;

namespace CadentManagement.TimeTracking.Dto;

[Serializable]
public class ImportCsvResultDto
{
    public int CreatedProjects { get; set; }

    public int UpdatedProjects { get; set; }

    public int CreatedTasks { get; set; }

    public int CreatedTimeEntries { get; set; }

    public int SkippedRows { get; set; }
}
