using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeachBiome : Biome
{
    public BeachBiome()
    {
        this.color = GetColorFromRGB(new Vector3(247, 219, 143));
    }
}
