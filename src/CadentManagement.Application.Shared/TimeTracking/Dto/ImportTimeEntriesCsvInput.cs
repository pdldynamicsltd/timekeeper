using System;
using System.ComponentModel.DataAnnotations;

namespace CadentManagement.TimeTracking.Dto;

[Serializable]
public class ImportTimeEntriesCsvInput
{
    [Required]
    public string CsvContent { get; set; }
}
