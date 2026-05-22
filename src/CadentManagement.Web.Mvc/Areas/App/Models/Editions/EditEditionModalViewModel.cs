using System.Collections.Generic;
using Abp.Application.Services.Dto;
using CadentManagement.Editions.Dto;
using CadentManagement.Web.Areas.App.Models.Common;

namespace CadentManagement.Web.Areas.App.Models.Editions;

public class EditEditionModalViewModel : GetEditionEditOutput, IFeatureEditViewModel
{
    public bool IsEditMode => Edition.Id.HasValue;

    public IReadOnlyList<ComboboxItemDto> EditionItems { get; set; }

    public IReadOnlyList<ComboboxItemDto> FreeEditionItems { get; set; }
}

