using Assets.World.Generator;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour, Generator
{
    [SerializeField]
    public float minimumCityDistanceRadius = 20f;

    List<GameObject> points;

    TerrainGenerator terrainGenerator;

    public Dictionary<Vector3, Color> coloredRays;

    public List<GameObject> houses;

    // Start is called before the first frame update
    void Start()
    {
        points = new List<GameObject>();
        coloredRays = new Dictionary<Vector3, Color>();

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
            Vector3 startPosition = new Vector3(pointPosition.x, 10, pointPosition.z);
            if (Physics.Raycast(startPosition, -transform.up, out RaycastHit tileHit, Mathf.Infinity))
            {
                // Per box make it raining rays
                Vector3 posi = tileHit.point;
                List<Vector3> rayHits = new List<Vector3>();
                int radius = 15;
                float margin = 0.3f;
                int minimumCitySize = 20;
                for (int x = (int)posi.x - radius; x < posi.x + radius; x++)
                {
                    for (int z = (int)posi.z - radius; z < posi.z + radius; z++)
                    {
                        Vector3 rayPosition = new Vector3(x, 10, z);
                        Ray ray = new Ray(rayPosition, -transform.up);
                        if (Physics.Raycast(ray, out RaycastHit hitInfo))
                        {

                            // Check if hit is within height margin
                            if (posi.y + margin >= hitInfo.point.y && posi.y - margin <= hitInfo.point.y && posi.y >= 0)
                            {
                                // Ray with a possibility for a city
                                rayHits.Add(hitInfo.point);
                                if (coloredRays.ContainsKey(hitInfo.point)) coloredRays[hitInfo.point] = Color.green;
                                else coloredRays.Add(hitInfo.point, Color.green);
                            }
                            else
                            {
                                if (coloredRays.ContainsKey(hitInfo.point)) coloredRays[hitInfo.point] = Color.red;
                                else coloredRays.Add(hitInfo.point, Color.red);
                            }

                        }

                    }
                }

                // TODO: Now only one house will spawn, this should be random (maybe based on size of city?)
                // Check if the city is large enough. If it is it will generate a city.
                if (rayHits.Count >= minimumCitySize)
                {
                    int fakeY = 0;
                    int randomHouseIndex = UnityEngine.Random.Range(0, houses.Count);
                    int randomLocationIndex = UnityEngine.Random.Range(0, rayHits.Count);

                    List<Vector3> rayHitsFakeY = new List<Vector3>();
                    foreach (var coord in rayHits)
                    {
                        rayHitsFakeY.Add(new Vector3(coord.x, fakeY, coord.z));
                    }

                    Vector3 houseBounds = CalculateBounds(houses[randomHouseIndex]);
                    Vector3 location = rayHits[randomLocationIndex];

                    Vector3 cornerCheck1 = new Vector3((float)Math.Round(location.x - houseBounds.x), fakeY, (float)Math.Round(location.z - houseBounds.z));
                    Vector3 cornerCheck2 = new Vector3((float)Math.Round(location.x + houseBounds.x), fakeY, (float)Math.Round(location.z + houseBounds.z));
                    Vector3 cornerCheck3 = new Vector3((float)Math.Round(location.x + houseBounds.x), fakeY, (float)Math.Round(location.z - houseBounds.z));
                    Vector3 cornerCheck4 = new Vector3((float)Math.Round(location.x - houseBounds.x), fakeY, (float)Math.Round(location.z + houseBounds.z));

                    //Debug.Log("Location: " + location + ", houseBounds: " + houseBounds);
                    //Debug.Log("cornerCheck1: " + cornerCheck1 + ", cornerCheck2: " + cornerCheck2 + ", cornerCheck3: " + cornerCheck3 + ", cornerCheck4: " + cornerCheck4);

                    Color color = Color.yellow;
                    if (coloredRays.ContainsKey(cornerCheck1)) coloredRays[cornerCheck1] = color;
                    else coloredRays.Add(cornerCheck1, color);
                    if (coloredRays.ContainsKey(cornerCheck2)) coloredRays[cornerCheck2] = color;
                    else coloredRays.Add(cornerCheck2, color);
                    if (coloredRays.ContainsKey(cornerCheck3)) coloredRays[cornerCheck3] = color;
                    else coloredRays.Add(cornerCheck3, color);
                    if (coloredRays.ContainsKey(cornerCheck4)) coloredRays[cornerCheck4] = color;
                    else coloredRays.Add(cornerCheck4, color);

                    // Check for every corner if it is within the green rays
                    if (rayHitsFakeY.Contains(cornerCheck1) && rayHitsFakeY.Contains(cornerCheck2) && rayHitsFakeY.Contains(cornerCheck3) && rayHitsFakeY.Contains(cornerCheck4))
                    {
                        color = Color.cyan;
                        if (coloredRays.ContainsKey(cornerCheck1)) coloredRays[cornerCheck1] = color;
                        else coloredRays.Add(cornerCheck1, color);
                        if (coloredRays.ContainsKey(cornerCheck2)) coloredRays[cornerCheck2] = color;
                        else coloredRays.Add(cornerCheck2, color);
                        if (coloredRays.ContainsKey(cornerCheck3)) coloredRays[cornerCheck3] = color;
                        else coloredRays.Add(cornerCheck3, color);
                        if (coloredRays.ContainsKey(cornerCheck4)) coloredRays[cornerCheck4] = color;
                        else coloredRays.Add(cornerCheck4, color);

                        // TODO: Rotation
                        GameObject building = Instantiate(houses[randomHouseIndex], new Vector3(location.x, houses[randomHouseIndex].transform.position.y + location.y, location.z), Quaternion.identity);

                        // TODO: Give building a parent 
                    }

                }

            }
            return true;
        }

        return false;
    }

    public Vector3 CalculateBounds(GameObject go)
    {
        // TODO: Make this method better...
        Bounds bounds = new Bounds();
        //go.size = Vector3.zero; // reset
        Collider[] colliders = go.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            bounds.Encapsulate(col.bounds);
        }
        return bounds.size;
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

        foreach (var ray in coloredRays)
        {
            Debug.DrawRay(ray.Key, transform.up * 10, ray.Value);
        }

    }


}
