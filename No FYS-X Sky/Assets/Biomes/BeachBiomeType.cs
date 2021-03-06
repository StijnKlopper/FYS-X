﻿using UnityEngine;

public class BeachBiomeType : BiomeType
{
    public BeachBiomeType()
    {
        this.BiomeTypeId = 6;
        this.Color = new Color(0, 0, 0, 0);
        this.Color2 = new Color(0, 1, 0, 0);
        this.Color3 = new Color(0, 0, 0, 0);
        this.HeightCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f, 0f, 0f), new Keyframe(0.4989361f, 0.0605761f, 0.06226838f, 0.06226838f, 0.3333333f, 0.1245139f), new Keyframe(1f, 0.3833351f, 0.8328113f, 0.8328113f, 0.1141188f, 0f));
    }
}
