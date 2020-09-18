using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Biome
{

    public Color color;

    public Color GetColorFromRGB(Vector3 color)
    {
        int limit = 255;
        return new Color(color.x / limit, color.y / limit, color.z / limit);
    }


}
