using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColdWetRegionType : RegionType
{

    public ColdWetRegionType()
    {
        this.AvailableBiomes = new List<BiomeType>();
        this.AvailableBiomes.Add(new MountainBiomeType());
        this.AvailableBiomes.Add(new SnowBiomeType());
        this.AvailableBiomes.Add(new TaigaBiomeType());
    }
}
