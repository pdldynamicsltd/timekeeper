using Abp.Mapperly;
using CadentManagement.Editions.Dto;
using CadentManagement.Web.Areas.App.Models.Editions;
using Riok.Mapperly.Abstractions;

namespace CadentManagement.Web.Mappers;

[Mapper]
public partial class GetEditionEditOutputToCreateEditionModalViewModelMapper : MapperBase<GetEditionEditOutput, CreateEditionModalViewModel>
{
    public override partial CreateEditionModalViewModel Map(GetEditionEditOutput source);

    public override partial void Map(GetEditionEditOutput source, CreateEditionModalViewModel destination);
}

[Mapper]
public partial class GetEditionEditOutputToEditEditionModalViewModelMapper : MapperBase<GetEditionEditOutput, EditEditionModalViewModel>
{
    public override partial EditEditionModalViewModel Map(GetEditionEditOutput source);

    public override partial void Map(GetEditionEditOutput source, EditEditionModalViewModel destination);
}
