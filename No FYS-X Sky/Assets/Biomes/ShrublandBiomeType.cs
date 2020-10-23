using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShrublandBiomeType : BiomeType
{
    public ShrublandBiomeType()
    {
        this.heightCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f, 0f, 0f), new Keyframe(0.496281f, 0.1427294f, 0.06226838f, 0.06226838f, 0.3333333f, 0.1245139f), new Keyframe(1.007996f, 0.4636536f, 0.8328113f, 0.8328113f, 0.1141188f, 0f));
        this.biomeTypeId = 7;
        this.color = new Color(0, 0, 0, 0);
        this.color2 = new Color(0, 0, 0, 1);
        this.color3 = new Color(0, 0, 0, 0);
    }
}
