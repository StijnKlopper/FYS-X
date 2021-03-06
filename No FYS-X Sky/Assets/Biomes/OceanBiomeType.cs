﻿using UnityEngine;

public class OceanBiomeType : BiomeType
{
    public OceanBiomeType()
    {
        this.BiomeTypeId = 0;
        this.HeightCurve = new AnimationCurve(new Keyframe(-1f, -1f, -2f, -2f, -1f, 0.3333333f), new Keyframe(1f, 0f, 0f, 0f, 0.3333333f, 0f));
        this.Color = new Color(1, 0, 0, 0);
        this.Color2 = new Color(0, 0, 0, 0);
        this.Color3 = new Color(0, 0, 0, 0);
    }
}
