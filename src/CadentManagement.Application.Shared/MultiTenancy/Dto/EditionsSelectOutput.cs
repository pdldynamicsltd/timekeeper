using System.Collections.Generic;
using CadentManagement.Editions.Dto;

namespace CadentManagement.MultiTenancy.Dto;

public class EditionsSelectOutput
{
    public EditionsSelectOutput()
    {
        AllFeatures = new List<FlatFeatureSelectDto>();
        EditionsWithFeatures = new List<EditionWithFeaturesDto>();
    }

    public List<FlatFeatureSelectDto> AllFeatures { get; set; }

    public List<EditionWithFeaturesDto> EditionsWithFeatures { get; set; }
}

