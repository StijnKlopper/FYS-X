using Assets.World.Generator;
using System;
using System.Collections.Generic;
using TreeEditor;
using UnityEditor.Profiling;
using UnityEngine;

public class CityGenerator : MonoBehaviour, Generator
{
    [SerializeField]
    public float minimumCityDistanceRadius = 150f; // TODO: Werkt niet? Cities moeten verder van elkaar af maar lijkt wel of dit niet werkt

    public int minimumCitySize = 20;

    public int cityRadius = 20;

    float margin = 0.5f;

    public bool raysDebug;

    TerrainGenerator terrainGenerator;

    public Dictionary<Vector3, CityPoint> cityPoints;

    public List<GameObject> houses;

    GameObject parentObj;

    void Start()
    {
        raysDebug = true; 

        cityPoints = new Dictionary<Vector3, CityPoint>();

        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        parentObj = GameObject.Find("CityPoints");
    }

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
                        cityPoints[cubePosition].ReplaceOrAddCityPointCoordinate(true, hitInfo.point);
                    }
                    else
                    {
                        // Invalid map coord positions
                        cityPoints[cubePosition].ReplaceOrAddCityPointCoordinate(false, hitInfo.point);
                    }

                }

            }
        }
    }

    public void GenerateCity(Vector3 cityCubeLocation)
    {
        int maxBuildingsPerCity = 4;
        int cityRaySize = cityPoints[cityCubeLocation].cityCoordinates.Count;

        // Check if the city is large enough. If it is it will generate a city.
        if (cityRaySize >= minimumCitySize)
        {

             //Place buildings untill no more space in the area
            int tries = 0;
            int currentCitySize = cityRaySize;
            while (currentCitySize >= minimumCitySize && tries <= maxBuildingsPerCity)
            {
                tries += 1;
                GenerateBuilding(cityCubeLocation, tries);
                currentCitySize = cityPoints[cityCubeLocation].cityCoordinates.Count;
            }
            
        }
        else
        {
            // Delete city because it isn't big enough
            cityPoints.Remove(cityCubeLocation);
        }
    }

    public void GenerateBuilding(Vector3 cityCubeLocation, int seed=1)
    {
        if (seed <= 0) seed = 1;

        List<Vector3> rayHits = cityPoints[cityCubeLocation].cityCoordinates;

        if (rayHits == null) return;

        int fakeY = 0;
        // TODO: Probleem is dat we niet huizen kunnen generen "random", ik dacht aan de seed die gebruikt kon worden maar dat kan niet want dan alsnog komt er in elke city dezelfde huizen.
        // Lijkt erop dat die cityCubeLocation.x en cityCubeLocation.z niet iets randoms genereren voor verschillende cities?
        int randomHouseIndex = (int)Math.Round(Mathf.PerlinNoise(cityCubeLocation.x, cityCubeLocation.z) * houses.Count);
        Debug.Log(Mathf.PerlinNoise(cityCubeLocation.x, cityCubeLocation.z)); // Tussen 0 en 1
        Debug.Log(houses.Count); // 2
        Debug.Log(randomHouseIndex); // 1
        Debug.Log("---");
        int randomLocationIndex = (int)Math.Round(Mathf.PerlinNoise(cityCubeLocation.x * seed, cityCubeLocation.z * seed) * rayHits.Count);

        List<Vector3> rayHitsFakeY = new List<Vector3>();
        foreach (var coord in rayHits)
        {
            rayHitsFakeY.Add(new Vector3(coord.x, fakeY, coord.z));
        }

        // Calculate bounds of the house
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
                    cityPoints[cityCubeLocation].ReplaceOrAddCityPointCoordinate(true, vectorToCheck);
                }
                else
                {
                    cityPoints[cityCubeLocation].ReplaceOrAddCityPointCoordinate(false, vectorToCheck);
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
            Vector3 targetPostition = new Vector3(cityCubeLocation.x, house.transform.position.y, cityCubeLocation.z);
            house.transform.LookAt(targetPostition);

            tile.AddObject(house);

            // Update coordinates
            UpdateCoordinatesAroundBox(cityCubeLocation);
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
        }
    }

}
