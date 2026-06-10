using System;
using System.ComponentModel.DataAnnotations;

namespace CadentManagement.TimeTracking.Dto;

[Serializable]
public class ImportProjectsCsvInput
{
    [Required]
    public string CsvContent { get; set; }
}
