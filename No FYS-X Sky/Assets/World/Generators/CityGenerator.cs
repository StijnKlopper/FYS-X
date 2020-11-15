using Assets.World.Generator;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour, Generator
{
    [SerializeField]
    public float minimumCityDistanceRadius = 20f;

    List<GameObject> points;

    //List<Vector3> vertexBuffer = new List<Vector3>();

    TerrainGenerator terrainGenerator;

    // Start is called before the first frame update
    void Start()
    {
        points = new List<GameObject>();
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

            int layerMask = 1 << 8;

            // This would cast rays only against colliders in layer 8.
            // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
            layerMask = ~layerMask;

            // Check height with Raycasting
            //Debug.Log(Physics.Raycast(cube.transform.position, -transform.up));
            RaycastHit Hit;
            
            if(Physics.Raycast(pointPosition, transform.up, out Hit, 20f, layerMask))
            {
                Debug.Log("IT WORKED" + Hit.transform.name + " INDEX IS " + Hit.triangleIndex);
            }
            Collider[] hitColliders = Physics.OverlapBox(pointPosition, transform.up);
            foreach (var hit in hitColliders)
            {
                Debug.Log("From: " + pointPosition + " To: " + hit.transform.name + "Closestpoint: " + hit.ClosestPoint(pointPosition) + "Contact offset: " +  hit.contactOffset );
                // List efficentier om te gebruiken??? handig voor optimization later
                //vertexBuffer.Clear();
                //hit.gameObject.GetComponent<MeshFilter>().sharedMesh.GetVertices(vertexBuffer)
                foreach (Vector3 index in hit.gameObject.GetComponent<MeshFilter>().mesh.vertices)
                {
                    Debug.Log("Localposition of the Vertex: " + index + "Worldposition of the Vertex: " + GetVertexWorldPosition(index, hit.transform));

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
    }

    void getHeight(Vector2 position)
    {
        int scale = 10;

        float sampleX = (position.x) / scale;
        float sampleY = (position.y) / scale;

        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
    }
   
}
