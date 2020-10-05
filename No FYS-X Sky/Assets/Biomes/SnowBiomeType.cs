using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SnowBiomeType : BiomeType
{
    public SnowBiomeType()
    {
        this.color = Color.white;

        this.color = GetColorFromRGB(new Vector3(89, 138, 37));
    }

}
