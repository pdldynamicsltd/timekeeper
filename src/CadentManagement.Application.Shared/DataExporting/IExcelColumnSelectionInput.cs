using System.Collections.Generic;

namespace CadentManagement.DataExporting;

public interface IExcelColumnSelectionInput
{
    List<string> SelectedColumns { get; set; }
}

