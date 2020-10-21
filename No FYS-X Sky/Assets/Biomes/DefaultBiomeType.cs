using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultBiomeType : BiomeType
{
    public DefaultBiomeType()
    { 
        this.textureIndex = 10;
        this.color = new Color(1.0f, 0, 0);
    }
}
