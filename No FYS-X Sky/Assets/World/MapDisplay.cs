using UnityEngine;
using UnityEngine.Tilemaps;
public class MapDisplay : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    public Renderer textureRender;




    public void GenerateMap()
    {
        float[,] noiseMap = TileGenerator.generateNoiseMapNew(mapWidth, mapHeight, noiseScale);

        Texture2D texture = new Texture2D(mapWidth, mapHeight);

        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {

            for (int x = 0; x < mapWidth; x++)
            {
                //Debug.Log(noiseMap[x, y]);
                //texture.SetPixel(x, y, Color.Lerp(Color.black, Color.white,(noiseMap[x, y] + noiseMap [x,y] * noiseMap[x,y])));
                colorMap[y * mapWidth + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        Debug.Log(colorMap);

        texture.SetPixels(colorMap);
        //texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(mapWidth, 1, mapHeight);

    }
}
