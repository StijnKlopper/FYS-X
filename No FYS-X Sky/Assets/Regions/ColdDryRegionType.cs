using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColdDryRegionType : RegionType
{

    public ColdDryRegionType()
    {
        this.availableBiomes = new List<BiomeType>();
        this.availableBiomes.Add(new MountainBiomeType());
        this.availableBiomes.Add(new SnowBiomeType());
        this.availableBiomes.Add(new TundraBiomeType());
    }
}
