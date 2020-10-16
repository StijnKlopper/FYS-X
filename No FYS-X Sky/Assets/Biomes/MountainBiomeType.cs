using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MountainBiomeType : BiomeType
{
    public MountainBiomeType()
    {
        this.color = Color.gray;
        this.heightCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f, 0f, 0f), new Keyframe(0.5069011f, 0.4546191f, 2.630252f, 2.630252f, 0.3333333f, 0.07337356f), new Keyframe(1.001126f, 1f, 0.8328113f, 0.8328113f, 0.1141188f, 0f));

        this.color = GetColorFromRGB(new Vector3(89, 138, 37));
        this.textureIndex = 2;
    }
}
