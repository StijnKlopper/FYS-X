using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotDryRegionType : RegionType
{

    public HotDryRegionType()
    {
        this.AvailableBiomes = new List<BiomeType>();
        this.AvailableBiomes.Add(new MountainBiomeType());
        this.AvailableBiomes.Add(new DesertBiomeType());
        this.AvailableBiomes.Add(new ShrublandBiomeType());
    }
}
