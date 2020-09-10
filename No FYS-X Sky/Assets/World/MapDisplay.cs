using UnityEngine;
using UnityEngine.Tilemaps;
public class MapDisplay : MonoBehaviour
{
    public enum DrawMode {NoiseMap, ColourMap, Mesh}
    public DrawMode drawMode;
    public int mapWidth;
    public int mapHeight;

    public float noiseScale;
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offsets;

    public bool autoUpdate;

    public Renderer textureRender;

    public TerrainType[] regions;

    public void GenerateMap()
    {
        TileGenerator tile = new TileGenerator();
        float[,] noiseMap = tile.GenerateNoiseMapNew(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offsets);

        Texture2D texture = new Texture2D(mapWidth, mapHeight);

        Color[] noiseMapC = new Color[mapWidth * mapHeight];
        Color[] colourMapC = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for(int i =0; i< regions.Length; i++)
                {
                    if(currentHeight <= regions[i].height)
                    {
                        colourMapC[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                }
                noiseMapC[y * mapWidth + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }
        if(drawMode == DrawMode.NoiseMap) texture.SetPixels(noiseMapC);
        if(drawMode == DrawMode.ColourMap) texture.SetPixels(colourMapC);
        //Prevents blurryness and makes the colours hold on to points
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(mapWidth, 1, mapHeight);
    }

    private void OnValidate()
    {
        if (mapWidth < 1) mapWidth = 1;
        if (mapHeight < 1) mapHeight = 1;
        if (octaves < 0) octaves = 0;
        if (lacunarity < 0) lacunarity = 0;
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;

}




