using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DesertBiomeType : BiomeType
{
    public DesertBiomeType()
    {
        this.color = GetColorFromRGB(new Vector3(181, 140, 34));
        this.heightCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f, 0f, 0f), new Keyframe(0.4922527f, 0.2189013f, 0.06226838f, 0.06226838f, 0.3333333f, 0.1245139f), new Keyframe(1.003974f, 0.3499472f, 0.8328113f, 0.8328113f, 0.1141188f, 0f));
        this.biomeTexture = new Texture2D(100, 100);
        byte[] textureData = File.ReadAllBytes("Assets/Textures/desertTexture.jpg");
        this.biomeTexture.LoadImage(textureData);
    }
}
