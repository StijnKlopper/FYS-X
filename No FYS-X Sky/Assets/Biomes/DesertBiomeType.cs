using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertBiomeType : BiomeType
{
    public DesertBiomeType()
    {
        this.color = GetColorFromRGB(new Vector3(181, 140, 34));
        this.heightMultiplier = 12f;
    }
}
