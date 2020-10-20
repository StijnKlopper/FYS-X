using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultBiomeType : BiomeType
{
    public DefaultBiomeType()
    { 
        this.color = GetColorFromRGB(new Vector3(255, 0, 255));
        this.textureIndex = 10;
    }
}
