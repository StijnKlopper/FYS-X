using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlainsBiomeType : BiomeType
{
    public PlainsBiomeType()
    {
        this.color = GetColorFromRGB(new Vector3(146, 201, 85));
    }
}
