using UnityEngine;
using LibNoise.Generator;

public class DecorationGenerator : MonoBehaviour
{
    private TerrainGenerator terrainGenerator;

    public static float threshold;

    public GameObject plants;
    // Start is called before the first frame update
    void Start()
    {
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        Debug.Log(threshold);
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void Generate(float[,] heightMap, Vector3 position)
    {
        for (int y = 0; y < heightMap.GetLength(0); y++)
        {
            for (int x = 0; x < heightMap.GetLength(0); x++)
            {
                //Debug.Log("X = " + x + "Y = " + y);
                if (heightMap[x, y] >= threshold)
                {
                    Biome biome = terrainGenerator.GetBiomeByCoordinates(new Vector2(position.x, position.z));
                    ////Biome biome = terrainGenerator.GetBiomeByCoordinates(new Vector2(x + offsets.x, y + offsets.y));
                    if (biome.BiomeType is DefaultBiomeType || biome.BiomeType is ForestBiomeType || biome.BiomeType is PlainsBiomeType || biome.BiomeType is ShrublandBiomeType)
                    {

                        Vector3 pos = new Vector3((position.x + x),heightMap[x, y], (position.z + y));
                        //Tile tile = WorldBuilder.GetTile(pos);
                        //Debug.Log(tile.HeightMap);
                        Instantiate(plants, pos, Quaternion.identity);
                       
                        //GameObject Tree = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        //Tree.transform.position = pos;
                    }

                }
            }
        }
        //float[,] noiseMap = generateNoiseMap(mapWidth, mapHeight, offsets);
        //for (int y = 0; y < mapHeight; y++)
        //{
        //    for (int x = 0; x < mapWidth; x++)
        //    {
        //        if(noiseMap[x,y] >= threshold)
        //        {
        //            Biome biome = terrainGenerator.GetBiomeByCoordinates(new Vector2(x + offsets.x, y + offsets.y));
        //            if (biome.BiomeType is DefaultBiomeType || biome.BiomeType is ForestBiomeType || biome.BiomeType is PlainsBiomeType || biome.BiomeType is ShrublandBiomeType)
        //            {
        //                Vector3 pos = new Vector3((x + offsets.x), 1,( y + offsets.y));
        //                //Tile tile = WorldBuilder.GetTile(pos);
        //                //Debug.Log(tile.HeightMap);
        //                GameObject Tree = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //                Tree.transform.position = pos;
        //            }
                    
        //        }
        //    }
        //}
    }

    private float[,] generateNoiseMap(int mapWidth, int mapHeight, Vector2 offsets)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];
        float frequency = 1f;
        float persistence = 1f;
        float lacunarity = 2f;

        int octaves = 1;
        float scale = 100.777f;
        Perlin perlin = new Perlin(frequency, lacunarity, persistence, octaves, terrainGenerator.Seed, LibNoise.QualityMode.High);

        for(int y = 0; y< mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                double sampleX = (x + offsets.x) / scale;
                double sampleY = (y + offsets.y) / scale;
                float noiseHeight = (float)(perlin.GetValue(sampleY, 0, sampleX) + 1) / 2;

                noiseMap[x, y] = noiseHeight;
                // Change height based on height curve and heightMultiplier
                //Biome biome = terrainGenerator.GetBiomeByCoordinates(new Vector2(x + offsets.x, y + offsets.y));
                

               
            }
        }
        return noiseMap;
    }
}
    
