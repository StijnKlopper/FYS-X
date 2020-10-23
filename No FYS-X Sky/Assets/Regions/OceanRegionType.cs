﻿using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanRegionType : RegionType
{

    public OceanRegionType()
    {
        this.availableBiomes = new List<BiomeType>();
        this.availableBiomes.Add(new OceanBiomeType());
        this.RegionColor = new Color(1, 0, 0, 0);
    }
}
