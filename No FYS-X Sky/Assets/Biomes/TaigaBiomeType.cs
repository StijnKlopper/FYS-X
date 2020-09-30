using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TaigaBiomeType : BiomeType
{
    public TaigaBiomeType()
    {
        this.color = GetColorFromRGB(new Vector3(0, 100, 0));
    }

}
