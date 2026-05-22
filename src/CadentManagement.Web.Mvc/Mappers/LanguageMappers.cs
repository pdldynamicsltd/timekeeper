using Abp.Mapperly;
using CadentManagement.Localization.Dto;
using CadentManagement.Web.Areas.App.Models.Languages;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Web.Mappers;

[Mapper]
public partial class GetLanguageForEditOutputToCreateOrEditLanguageModalViewModelMapper : MapperBase<GetLanguageForEditOutput, CreateOrEditLanguageModalViewModel>
{
    public override partial CreateOrEditLanguageModalViewModel Map(GetLanguageForEditOutput source);
    public override partial void Map(GetLanguageForEditOutput source, CreateOrEditLanguageModalViewModel destination);
}
