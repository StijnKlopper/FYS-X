using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotDryRegionType : RegionType
{

    public HotDryRegionType()
    {
        this.availableBiomes = new List<BiomeType>();
        this.availableBiomes.Add(new MountainBiomeType());
        this.availableBiomes.Add(new DesertBiomeType());
        this.availableBiomes.Add(new ShrublandBiomeType());
        this.RegionColor = new Color(0, 0, 1, 0);
    }
}
