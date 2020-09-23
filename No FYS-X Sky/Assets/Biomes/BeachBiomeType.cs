using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeachBiomeType : BiomeType
{
    public BeachBiomeType()
    {
        this.color = GetColorFromRGB(new Vector3(247, 219, 143));
        this.heightMultiplier = 8f;
        this.yOffset = -2;
    }
}
