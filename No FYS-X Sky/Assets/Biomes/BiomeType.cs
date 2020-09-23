using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BiomeType
{
    public Color color;

    public float heightMultiplier;

    public float yOffset = 0;

    public Color GetColorFromRGB(Vector3 color)
    {
        int limit = 255;
        return new Color(color.x / limit, color.y / limit, color.z / limit);
    }
}
