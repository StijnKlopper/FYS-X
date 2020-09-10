using UnityEngine;
using UnityEngine.Tilemaps;
public class MapDisplay : MonoBehaviour
{
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

    public void GenerateMap()
    {
        TileGenerator tile = new TileGenerator();
        float[,] noiseMap = tile.GenerateNoiseMapNew(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offsets);

        Texture2D texture = new Texture2D(mapWidth, mapHeight);

        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                colorMap[y * mapWidth + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        texture.SetPixels(colorMap);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(mapWidth, 1, mapHeight);
    }

    private void OnValidate()
    {
        if (mapWidth < 1) mapWidth = 1;
        if (mapHeight < 1) mapHeight = 1;
        if (octaves < 0) octaves = 1;
    }
}
