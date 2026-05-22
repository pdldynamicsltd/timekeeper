using System.Collections.Generic;
using Abp.Application.Services.Dto;
using CadentManagement.Editions.Dto;

namespace CadentManagement.Web.Areas.App.Models.Common;

public interface IFeatureEditViewModel
{
    List<NameValueDto> FeatureValues { get; set; }

    List<FlatFeatureDto> Features { get; set; }
}

