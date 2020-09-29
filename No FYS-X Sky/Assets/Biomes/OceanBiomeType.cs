using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class OceanBiomeType : BiomeType
{
    public OceanBiomeType()
    {
        this.color = GetColorFromRGB(new Vector3(67, 183, 222));
        this.heightCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f, 0f, 0.3333333f), new Keyframe(1f, 0f, 0f, 0f, 0.3333333f, 0f));

        this.color = GetColorFromRGB(new Vector3(89, 138, 37));
        this.biomeTexture = new Texture2D(100, 100);
        byte[] textureData = File.ReadAllBytes("Assets/Textures/waterTexture.jpg");
        this.biomeTexture.LoadImage(textureData);
    }
}
