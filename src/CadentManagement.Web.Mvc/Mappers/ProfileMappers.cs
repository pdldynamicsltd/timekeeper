using Abp.Mapperly;
using CadentManagement.Authorization.Users.Profile.Dto;
using CadentManagement.Web.Areas.App.Models.Profile;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Web.Mappers;

[Mapper]
public partial class CurrentUserProfileEditDtoToMySettingsViewModelMapper : MapperBase<CurrentUserProfileEditDto, MySettingsViewModel>
{
    public override partial MySettingsViewModel Map(CurrentUserProfileEditDto source);

    public override partial void Map(CurrentUserProfileEditDto source, MySettingsViewModel destination);
}
