using CadentManagement.Localization.Dto;

namespace CadentManagement.Web.Areas.App.Models.Languages;

public class CreateOrEditLanguageModalViewModel : GetLanguageForEditOutput
{
    public bool IsEditMode => Language.Id.HasValue;
}

