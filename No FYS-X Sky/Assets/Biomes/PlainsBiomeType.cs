using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlainsBiomeType : BiomeType
{
    public PlainsBiomeType()
    {
        this.color = GetColorFromRGB(new Vector3(146, 201, 85));

        this.color = GetColorFromRGB(new Vector3(89, 138, 37));
        this.biomeTexture = new Texture2D(100, 100);
        byte[] textureData = File.ReadAllBytes("Assets/Textures/grassTexture.jpeg");
        this.biomeTexture.LoadImage(textureData);
    }
}
