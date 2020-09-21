using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanBiomeType : BiomeType
{
    public OceanBiomeType()
    {
        this.color = GetColorFromRGB(new Vector3(67, 183, 222));
    }
}
