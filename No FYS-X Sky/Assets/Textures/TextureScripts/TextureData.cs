using System.Linq;
using UnityEngine;

[CreateAssetMenu()]
public class TextureData : UpdatableData
{

    const int TEXTURE_SIZE = 512;
    const TextureFormat TEXTURE_FORMAT = TextureFormat.RGB565;

    public Layer[] layers;

    // Set values for every texture layer and apply to material
    public void ApplyToMaterial(Material material) {
        Texture2DArray texturesArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());
        material.SetTexture("_BaseTextures", texturesArray);
        material.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());
    }

    [System.Serializable]
    public class Layer {
        public Texture2D texture;
        [Range(0.1f, 10.0f)]
        public float textureScale = 1;
    }

    // Generate texture array data type from array of 2D textures
    Texture2DArray GenerateTextureArray(Texture2D[] textures) {
        Texture2DArray textureArray = new Texture2DArray(TEXTURE_SIZE, TEXTURE_SIZE, textures.Length, TEXTURE_FORMAT, true);
        for (int i = 0; i < textures.Length; i++) {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }
        textureArray.Apply();
        return textureArray;
    }

}
