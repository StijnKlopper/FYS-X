using Assets.World.Generator;
using System;
using System.Collections.Generic;
using UnityEditor.Profiling;
using UnityEngine;

public class CityGenerator : MonoBehaviour, Generator
{
    [SerializeField]
    public float minimumCityDistanceRadius = 20f;

    public int minimumCitySize = 20;

    public int cityRadius = 20;

    float margin = 0.5f;

    public bool raysDebug = false;

    TerrainGenerator terrainGenerator;

    public Dictionary<Vector3, Color> coloredRays;

    public Dictionary<Vector3, CityPoint> cityPoints;

    public List<GameObject> houses;

    GameObject parentObj;

    void Start()
    {
        coloredRays = new Dictionary<Vector3, Color>();
        cityPoints = new Dictionary<Vector3, CityPoint>();

        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        parentObj = GameObject.Find("CityPoints");
    }

    // Update is called once per frame
    void Update()
    {
        DebugPoints();
    }

    public void Generate(int mapWidth, int mapHeight, Vector2 offsets)
    {
        // Check from center of tile if there is a city point that needs to be updated
        CheckForUpdateNearbyCityPoints(new Vector3( -(offsets.x + (mapWidth / 2)), 0, -(offsets.y + (mapHeight / 2))));

        // Loop through the cubes and check with rays if possible locations
        List<Vector3> cubePoints = DrawCityLocations(mapWidth, mapHeight, offsets);
        foreach (Vector3 cubePoint in cubePoints)
        {
            UpdateCoordinatesAroundBox(cubePoint);
            GenerateCity(cubePoint);
        }
    }

    public void CheckForUpdateNearbyCityPoints(Vector3 coordinateToCheck)
    {
        CityPoint nearestCityLocation = GetNearestCityLocation(coordinateToCheck);

        // If a nearby city exists and is nearby enough
        if (nearestCityLocation != null)
        {
            UpdateCoordinatesAroundBox(nearestCityLocation.cubePosition);
        }
    }

    public CityPoint GetNearestCityLocation(Vector3 nearbyCoordinate)
    {
        // Get the closest city cube location
        CityPoint closestCityPoint = null;
        float distanceClosestCityPoint = float.NaN;
        foreach (var cityPoint in cityPoints)
        {
            float distance = Vector3.Distance(nearbyCoordinate, cityPoint.Key);
            if (distance != 0 && (float.IsNaN(distanceClosestCityPoint) || distance < distanceClosestCityPoint))
            {
                closestCityPoint = cityPoint.Value;
                distanceClosestCityPoint = distance;
            }
        }

        // Only returns if city is actually close
        if (distanceClosestCityPoint <= cityRadius * 1.5)
        {
            return closestCityPoint;
        }

        return null;
    }

    Vector3 CheckValidCityPoint(Vector3 pointPosition)
    {
        Vector2 cubePosition = new Vector2(-pointPosition.x, -pointPosition.z);
        Biome biome = terrainGenerator.GetBiomeByCoordinates(cubePosition);

        // Check for not a Ocean or Mountain biome
        if (!(biome.biomeType is OceanBiomeType) && !(biome.biomeType is MountainBiomeType))
        {
            Vector3 startPosition = new Vector3(pointPosition.x, 10, pointPosition.z);
            if (Physics.Raycast(startPosition, -transform.up, out RaycastHit tileHit, Mathf.Infinity))
            {
                return tileHit.point;
            }
        }
        return Vector3.zero;
    }

    public void UpdateCoordinatesAroundBox(Vector3 cubePosition)
    {
        // Per box make it raining rays to check for valid points
        for (int x = (int)cubePosition.x + cityRadius; x >= (int)cubePosition.x - cityRadius; x--)
        {
            for (int z = (int)cubePosition.z + cityRadius; z >= (int)cubePosition.z - cityRadius; z--)
            {
                Vector3 rayPosition = new Vector3(x, 10, z);
                Ray ray = new Ray(rayPosition, -transform.up);
                if (Physics.Raycast(ray, out RaycastHit hitInfo))
                {
                    // Check if hit is within height margin
                    if (cubePosition.y + margin >= hitInfo.point.y && cubePosition.y - margin <= hitInfo.point.y && cubePosition.y >= 0)
                    {
                        // Ray with a possibility for a city
                        ReplaceOrAddCityPointCoordinateAndRays(true, cubePosition, hitInfo.point, Color.green);
                    }
                    else
                    {
                        // Invalid map coord positions
                        ReplaceOrAddCityPointCoordinateAndRays(false, cubePosition, hitInfo.point, Color.red);
                    }

                }

            }
        }
    }

    public void ReplaceOrAddCityPointCoordinateAndRays(bool validCoord, Vector3 cubePosition, Vector3 coordinate, Color rayColor)
    {
        cityPoints[cubePosition].ReplaceOrAddCityPointCoordinate(validCoord, coordinate);

        // Add colored ray if is set
        if (rayColor != null && raysDebug == true)
        {
            if (coloredRays.ContainsKey(coordinate))
            {
                coloredRays[coordinate] = rayColor;
            }
            else
            {
                coloredRays.Add(coordinate, rayColor);
            }
        }
    }

    public void GenerateCity(Vector3 cityCubeLocation)
    {
        int cityRaySize = cityPoints[cityCubeLocation].cityCoordinates.Count;

        // Check if the city is large enough. If it is it will generate a city.
        if (cityRaySize >= minimumCitySize)
        {
            // TODO: Now hardcoded spawns, this should be random (maybe based on size of city?)
            for (int i = 0; i <= 10; i++)
            {
                GenerateBuilding(cityCubeLocation);
                // TODO: Call method below to update coords of city so no buildings can be inside eachother, this isn't working correctly atm 
                //UpdateCoordinatesAroundBox(cityCubeLocation);
            }
        }
    }

    public void GenerateBuilding(Vector3 cityCubeLocation)
    {
        List<Vector3> rayHits = cityPoints[cityCubeLocation].cityCoordinates;

        if (rayHits == null) return;

        int fakeY = 0;
        int randomHouseIndex = UnityEngine.Random.Range(0, houses.Count);
        int randomLocationIndex = UnityEngine.Random.Range(0, rayHits.Count);

        List<Vector3> rayHitsFakeY = new List<Vector3>();
        foreach (var coord in rayHits)
        {
            rayHitsFakeY.Add(new Vector3(coord.x, fakeY, coord.z));
        }

        //Kan netter
        Bounds houseB = CalculateBounds(houses[randomHouseIndex]);
        Vector3 houseBounds = houseB.size;
        Vector3 location = rayHits[randomLocationIndex];

        // All coordinates (corners) to check within
        int smallestX = (int)Math.Round(location.x - (houseBounds.x / 2));
        int highestX = (int)Math.Round(location.x + (houseBounds.x / 2));
        int smallestZ = (int)Math.Round(location.z - (houseBounds.z / 2));
        int highestZ = (int)Math.Round(location.z + (houseBounds.z / 2));

        bool valid = true;
        for (int x = smallestX; x <= highestX; x++)
        {
            for (int z = smallestZ; z <= highestZ; z++)
            {
                Vector3 vectorToCheck = new Vector3(x, fakeY, z);
                
                if (rayHitsFakeY.Contains(vectorToCheck)) 
                {
                    ReplaceOrAddCityPointCoordinateAndRays(true, cityCubeLocation, vectorToCheck, Color.white);
                }
                else
                {
                    ReplaceOrAddCityPointCoordinateAndRays(false, cityCubeLocation, vectorToCheck, Color.blue);
                    valid = false;
                }
            }
        }

        if (valid)
        {
            // Houses are placed from the centre of a prefab
            Tile tile = WorldBuilder.GetTile(new Vector3(location.x, 0, location.z));

            // Make House only rotate on Y axis.
            Vector3 housePosition = new Vector3(location.x - houseB.center.x, houses[randomHouseIndex].transform.position.y + location.y, location.z - houseB.center.z);
            GameObject house = Instantiate(houses[randomHouseIndex], housePosition, Quaternion.identity, parentObj.transform.GetChild(0).transform) as GameObject;
            house.transform.localRotation.SetLookRotation(cityCubeLocation);

            tile.AddObject(house);
        }

    }

    public Bounds CalculateBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1, ni = renderers.Length; i < ni; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }
        else
        {
            return new Bounds();
        }
    }

    public Vector3 GetVertexWorldPosition(Vector3 vertex, Transform owner)
    {
        // TODO: Method mag weg?
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

    // Create starting search points for city and place a cube
    public List<Vector3> DrawCityLocations(int mapWidth, int mapHeight, Vector2 offsets)
    {
        List<Vector3> cubePoints = new List<Vector3>();
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
                        Vector3 cubePoint = CheckValidCityPoint(possiblePointPosition);
                        if (cubePoint != Vector3.zero)
                        {
                            // Make a cube on the valid location
                            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            point.transform.SetParent(parentObj.transform.GetChild(1).transform);
                            point.transform.position = possiblePointPosition;
                            point.layer = LayerMask.NameToLayer("Ignore Raycast");
                            point.GetComponent<Renderer>().enabled = false;
                            Physics.SyncTransforms();

                            // Add the cube point to an array
                            cityPoints.Add(cubePoint, new CityPoint(cubePoint));
                            cubePoints.Add(cubePoint);

                            break;
                        }
                    }
                }
            }
        }

        return cubePoints;
    }

    // Raycast a sphere and check for nearby gameobjects
    private bool checkNearbyPoints(Vector3 center, float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);
        foreach (var hitCollider in hitColliders)
        {
            // If gameobject is a cube then return true
            if (hitCollider.gameObject.name == ("Cube"))
            {
                return true;
            }
        }
        return false;
    }

    private void DebugPoints()
    {
        if (raysDebug)
        {
            foreach (var ray in coloredRays)
            {
                Debug.DrawRay(ray.Key, transform.up * 10, ray.Value);
            }

            /*
            foreach (var cityPoint in cityPoints)
            {

                // Valid city coordinates
                foreach (var coordinate in cityPoint.Value.cityCoordinates)
                {
                    Debug.DrawRay(coordinate, transform.up * 10, Color.green);
                }

                // Invalid city coordinates
                foreach (var coordinate in cityPoint.Value.invalidCityCoordinates)
                {
                    Debug.DrawRay(coordinate, transform.up * 10, Color.red);
                }

            }
            */
        }
    }

}
