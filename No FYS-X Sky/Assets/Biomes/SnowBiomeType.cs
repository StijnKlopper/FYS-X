using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SnowBiomeType : BiomeType
{
    public SnowBiomeType()
    {
        this.color = Color.white;

        this.color = GetColorFromRGB(new Vector3(89, 138, 37));
        this.biomeTexture = new Texture2D(100, 100);
        byte[] textureData = File.ReadAllBytes("Assets/Textures/snowTexture.jpeg");
        this.biomeTexture.LoadImage(textureData);
    }

}
