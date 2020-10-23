using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BiomeType
{
    public Color color;
    public Color color2;
    public Color color3;

    public AnimationCurve heightCurve = new AnimationCurve(new Keyframe(0f, 1f, 0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f, 0f, 0f));

    public int biomeTypeId;

    public Color GetColorFromRGB(Vector3 color)
    {
        int limit = 255;
        return new Color(color.x / limit, color.y / limit, color.z / limit);
    }
}
