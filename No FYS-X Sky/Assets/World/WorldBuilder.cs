using System.Collections.Generic;
using UnityEngine;

namespace Assets.World 
{
    class WorldBuilder : MonoBehaviour
    {
        public GameObject plane;
        private int seed;
        private List<GameObject> planeList;

        [SerializeField]
        private MeshRenderer tileRenderer;

        [SerializeField]
        public MeshFilter meshFilter;

        [SerializeField]
        private MeshCollider meshCollider;

        [SerializeField]
        private float mapScale;

        [SerializeField]
        private float heightMultiplier;

        [SerializeField]
        private AnimationCurve heightCurve;

        [SerializeField]
        private TerrainType[] terrainTypes;



        private void GenerateWorld()
        {

        }

        void Start()
        {
            GenerateTile();

            //plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            //plane.transform.position = new Vector3(0, 0, 0);
            //planeList = new List<GameObject>();
            //planeList.Add(plane);
        }

        void Update()
        {

        }


        private void GenerateWorld(Vector3 position) 
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.position = position;
            planeList.Add(plane);
        }

        private float[,] GenerateNoiseMap(int mapDepth, int mapWidth, float scale, float offsetX, float offsetZ)
        {
            float[,] noiseMap = new float[mapDepth, mapWidth];

            for (int zIndex = 0; zIndex < mapDepth; zIndex++)
            {
                for (int xIndex = 0; xIndex < mapWidth; xIndex++)
                {
                    float sampleX = (xIndex + offsetX) / scale;
                    float sampleZ = (zIndex + offsetZ) / scale;

                    float noise = Mathf.PerlinNoise(sampleX, sampleZ);

                    noiseMap[zIndex, xIndex] = noise;
                }
            }
            return noiseMap;
        }

        private void GenerateTile()
        {
            Vector3[] meshVertices = this.meshFilter.mesh.vertices;
            int tileDepth = (int)Mathf.Sqrt(meshVertices.Length);
            int tileWidth = tileDepth;

            float offsetX = -this.gameObject.transform.position.x;
            float offsetZ = -this.gameObject.transform.position.z;

            float[,] heightMap = GenerateNoiseMap(tileDepth, tileWidth, this.mapScale, offsetX, offsetZ);

            Texture2D tileTexture = BuildTexture(heightMap);
            this.tileRenderer.material.mainTexture = tileTexture;
            UpdateMeshVertices(heightMap);
        }

        private Texture2D BuildTexture(float[,] heightMap)
        {
            int tileDepth = heightMap.GetLength(0);
            int tileWidth = heightMap.GetLength(1);

            Color[] colorMap = new Color[tileDepth * tileWidth];
            for (int zIndex = 0; zIndex < tileDepth; zIndex++)
            {
                for (int xIndex = 0; xIndex < tileWidth; xIndex++)
                {
                    int colorIndex = zIndex * tileWidth + xIndex;
                    float height = heightMap[zIndex, xIndex];
                    TerrainType terrainType = ChooseTerrainType(height);
                    colorMap[colorIndex] = terrainType.color;
                }
            }

            Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
            tileTexture.wrapMode = TextureWrapMode.Clamp;
            tileTexture.SetPixels(colorMap);
            tileTexture.Apply();

            return tileTexture;
        }

        private TerrainType ChooseTerrainType(float height)
        {
            foreach (TerrainType terrainType in terrainTypes)
            {
                if (height < terrainType.height)
                {
                    return terrainType;
                }
            }
            return terrainTypes[terrainTypes.Length - 1];
        }

        [System.Serializable]
        public class TerrainType
        {
            public string name;
            public float height;
            public Color color;
        }

        private void UpdateMeshVertices(float[,] heightMap)
        {
            int tileDepth = heightMap.GetLength(0);
            int tileWidth = heightMap.GetLength(1);

            Vector3[] meshVertices = this.meshFilter.mesh.vertices;

            int vertexIndex = 0;

            for (int zIndex = 0; zIndex < tileDepth; zIndex++)
            {
                for (int xIndex = 0; xIndex < tileWidth; xIndex++)
                {
                    float height = heightMap[zIndex, xIndex];
                    Vector3 vertex = meshVertices[vertexIndex];
                    meshVertices[vertexIndex] = new Vector3(vertex.x, this.heightCurve.Evaluate(height) * this.heightMultiplier, vertex.z);

                    vertexIndex++;
                }
            }

            this.meshFilter.mesh.vertices = meshVertices;
            this.meshFilter.mesh.RecalculateBounds();
            this.meshFilter.mesh.RecalculateNormals();

            this.meshCollider.sharedMesh = this.meshFilter.mesh;
        }
    }
}
