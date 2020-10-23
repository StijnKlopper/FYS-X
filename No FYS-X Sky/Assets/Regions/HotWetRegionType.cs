using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotWetRegionType : RegionType
{

    public HotWetRegionType()
    {
        this.availableBiomes = new List<BiomeType>();
        this.availableBiomes.Add(new MountainBiomeType());
        this.availableBiomes.Add(new ForestBiomeType());
        this.availableBiomes.Add(new PlainsBiomeType());
        this.RegionColor = new Color(0, 1, 0, 0);

    }
}
