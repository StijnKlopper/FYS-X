using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultBiomeType : BiomeType
{
    public DefaultBiomeType()
    { 
        this.biomeTypeId = 10;
        this.color = new Color(0, 0, 0, 0);
        this.color2 = new Color(0, 0, 0, 0);
        this.color3 = new Color(0, 0, 0, 1);
    }
}
