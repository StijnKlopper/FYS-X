using Assets.World.Generator;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour, Generator
{
    [SerializeField]
    public float minimumCityDistanceRadius = 20f;

    List<GameObject> points;

    TerrainGenerator terrainGenerator;

    List<Vector3> possibleCoordsForCities;

    public List<GameObject> houses;

    // Start is called before the first frame update
    void Start()
    {
        points = new List<GameObject>();
        possibleCoordsForCities = new List<Vector3>();

        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        DebugPoints();
    }

    bool CheckValidPoint(Vector3 pointPosition)
    {
        Vector2 cubePosition = new Vector2(-pointPosition.x, -pointPosition.z);
        Biome biome = terrainGenerator.GetBiomeByCoordinates(cubePosition);

        // Check for not a Ocean or Mountain biome
        if (!(biome.biomeType is OceanBiomeType) && !(biome.biomeType is MountainBiomeType))
        {
            // Check height with Raycasting
            Collider[] hitColliders = Physics.OverlapBox(pointPosition, transform.up);
            foreach (var hit in hitColliders)
            {

                // Per box make it raining rays
                Vector3 posi = hit.transform.position;
                List<RaycastHit> rayHits = new List<RaycastHit>();
                int radius = 20;
                float margin = 1f;
                for (int x = (int) posi.x - radius; x < posi.x + radius; x++)
                {
                    for (int z = (int)posi.z - radius; z < posi.z + radius; z++)
                    {
                        Vector3 rayPosition = new Vector3(x, 10, z);
                        Ray ray = new Ray(rayPosition, Vector3.down);
                        if (Physics.Raycast(ray, out RaycastHit hitInfo)) {

                            // Check if hit is within height margin
                            if (posi.y + margin >= hitInfo.point.y && posi.y - margin <= hitInfo.point.y)
                            {
                                // Ray with a possibility for a city
                                rayHits.Add(hitInfo);
                                possibleCoordsForCities.Add(hitInfo.point);

                                // TODO
                                int rand = Random.Range(1, 1000);
                                if (rand <= 1)
                                {
                                    int houseRand = Random.Range(0, houses.Count);
                                    Debug.Log(houseRand);
                                    Instantiate(houses[houseRand], new Vector3(hitInfo.point.x, houses[houseRand].transform.position.y + hitInfo.point.y, hitInfo.point.z), Quaternion.identity);
                                }
                            }

                        }

                    }
                }




            }
            return true;
        }

        return false;
    }

    public Vector3 GetVertexWorldPosition(Vector3 vertex, Transform owner)
    {
        return owner.localToWorldMatrix.MultiplyPoint3x4(vertex);
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
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                if (currentHeight <= pointHeight)
                {
                    Vector3 possiblePointPosition = new Vector3(-(x + offsets.x), 0, -(y + offsets.y));

                    if (checkNearbyPoints(possiblePointPosition, minimumCityDistanceRadius))
                    {
                        break;
                    }
                    else
                    {

                        if (CheckValidPoint(possiblePointPosition))
                        {
                            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            point.transform.SetParent(parentObj.transform);
                            point.transform.position = possiblePointPosition;
                            //point.GetComponent<Renderer>().enabled = false;

                            Physics.SyncTransforms();

                            points.Add(point);
                            break;
                        }
                           
                    }
                }
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

    private void DebugPoints()
    {
        foreach (var cube in points)
        {
            Debug.DrawRay(cube.transform.position, -transform.up * 10, Color.green);
        }

        foreach (var coord in possibleCoordsForCities)
        {
            
            Debug.DrawRay(coord, transform.up * 30, Color.green);
        }
    }

    void getHeight(Vector2 position)
    {
        int scale = 10;

        float sampleX = (position.x) / scale;
        float sampleY = (position.y) / scale;

        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
    }
   
}
