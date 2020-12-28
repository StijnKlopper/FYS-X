using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultBiomeType : BiomeType
{
    public DefaultBiomeType()
    { 
        this.BiomeTypeId = 10;
        this.Color = new Color(0, 0, 0, 0);
        this.Color2 = new Color(0, 0, 0, 0);
        this.Color3 = new Color(0, 0, 0, 1);
    }
}
