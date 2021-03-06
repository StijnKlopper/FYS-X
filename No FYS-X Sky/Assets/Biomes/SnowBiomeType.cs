﻿using UnityEngine;

public class SnowBiomeType : BiomeType
{
    public SnowBiomeType()
    {
        this.BiomeTypeId = 4;
        this.HeightCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f, 0f, 0f), new Keyframe(0.5069011f, 0.4546191f, 2.630252f, 2.630252f, 0.3333333f, 0.07337356f), new Keyframe(1.001126f, 1f, 0.8328113f, 0.8328113f, 0.1141188f, 0f));
        this.Color = new Color(0, 0, 0, 0);
        this.Color2 = new Color(1, 0, 0, 0);
        this.Color3 = new Color(0, 0, 0, 0);
    }

}
