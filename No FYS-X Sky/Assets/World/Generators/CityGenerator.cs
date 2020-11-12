using Assets.World.Generator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour, Generator
{
    //public int totalpoints = 10;
    //public float buildingWith = 100f;
    //public float buildingLength = 100f;
    [SerializeField]
    public float minimumCityDistanceRadius = 20f;


    List<GameObject> points;

    TerrainGenerator terrainGenerator;

    Biome biome;
    // Start is called before the first frame update
    void Start()
    {
        points = new List<GameObject>();
    }

    void checkTiles(Vector2 coords)
    {

        biome = terrainGenerator.GetBiomeByCoordinates(coords);
        if (biome.biomeType is DesertBiomeType)
        {


        }

    }

    private float[,] generateCityNoiseMap(int mapWidth, int mapHeight, Vector2 offsets)
    {

        int scale = 20;
        int octaves = 1;
        mapWidth *= 5;
        mapWidth *= 5;
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;


        float[,] noiseMap = new float[mapWidth, mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                float amplitude = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + offsets.x) / scale;
                    float sampleY = (y + offsets.y) / scale;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
    //Create starting search points for city
    public void DrawCityLocations(int mapWidth, int mapHeight, Vector2 offsets)
    {
        GameObject parentObj = GameObject.Find("CityPoints");
        float[,] noiseMap = generateCityNoiseMap(mapWidth, mapHeight, offsets);

        float pointHeight = 0.00f;
        List<GameObject> cubes = new List<GameObject>();
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                if (currentHeight <= pointHeight)
                {
                    if (checkNearbyPoints(new Vector3(-(x + offsets.x), 10, -(y + offsets.y)), minimumCityDistanceRadius))
                    {
                        break;
                    }
                    else
                    {
                        GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        point.transform.SetParent(parentObj.transform);
                        point.transform.position = new Vector3(-(x + offsets.x), 10, -(y + offsets.y));
                        point.GetComponent<Renderer>().enabled = false;
                        cubes.Add(point);
                        Physics.SyncTransforms();
                        points.Add(point);
                        break;
                    }
                }

                //noiseMapC[y * mapWidth + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

    }
    //Raycast a sphere and check for nearby gameobjects
    private bool checkNearbyPoints(Vector3 center, float radius)
    {

        Collider[] hitColliders = Physics.OverlapSphere(center, radius);
        foreach (var hitCollider in hitColliders)
        {
            //If gameobject is a cube then return true
            if (hitCollider.gameObject.name == ("Cube"))
            {
                return true;
            }


        }
        return false;
    }

    //void OnDrawGizmoSelecteD()
    //{
    //    foreach (var cube in points)
    //    {
    //        Gizmos.DrawLine(cube.transform.position, -transform.up);
    //    }

    //}
    private void DebugPoints()
    {
        foreach (var cube in points)
        {
            Debug.DrawRay(cube.transform.position, -transform.up * 10, Color.green);
        }
    }

    void getHeight(Vector2 position)
    {
        int scale = 10;


        float sampleX = (position.x) / scale;
        float sampleY = (position.y) / scale;

        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
    }

    // Update is called once per frame
    void Update()
    {

        DebugPoints();

    }
}
