using UnityEngine;

public abstract class BiomeType
{
    public Color Color;
    public Color Color2;
    public Color Color3;

    public AnimationCurve HeightCurve = new AnimationCurve(new Keyframe(0f, 1f, 0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f, 0f, 0f));

    public int BiomeTypeId;

    public Color GetColorFromRGB(Vector3 color)
    {
        int limit = 255;
        return new Color(color.x / limit, color.y / limit, color.z / limit);
    }
}
