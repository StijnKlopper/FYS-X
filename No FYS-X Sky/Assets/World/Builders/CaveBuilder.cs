using Assets.World.Generator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;

public class CaveBuilder : MonoBehaviour
{

    [SerializeField]
    private MeshRenderer tileRenderer;

    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private MeshCollider meshCollider;

    RidgedMultifractal ridgedMultifractal;
    GameObject cavePlane;
    Texture2D noiseTexture;
    Color[] colorMap;

    int[] tempArray;


    // Start is called before the first frame update
    void Start()
    {
        GenerateCaveMap();
    }


    public void GenerateCaveMap() {
        cavePlane = GameObject.Find("CavePlane");

        noiseTexture = new Texture2D(10, 10);
        colorMap = new Color[10 * 10];

        tempArray = new int[10 * 10];

        float scale = 1000.17777f;

        Vector2 offsets = new Vector2(-this.gameObject.transform.position.x, -this.gameObject.transform.position.z);

        ridgedMultifractal = new RidgedMultifractal();
        ridgedMultifractal.OctaveCount = 10;
        ridgedMultifractal.Frequency = 2;


        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                int colorIndex = y * 10 + x;
                double tempVal = ridgedMultifractal.GetValue(x + offsets.x + scale, 1, y + offsets.y + scale);
                int isCave = tempVal < 0.0 ? 0 : 1;
                tempArray[colorIndex] = isCave;
                colorMap[colorIndex] = new Color(isCave, isCave, isCave);
            }
        }

        noiseTexture.filterMode = FilterMode.Point;
        noiseTexture.SetPixels(colorMap);
        noiseTexture.wrapMode = TextureWrapMode.Clamp;
        noiseTexture.Apply();



        tileRenderer.material.mainTexture = noiseTexture;

/*        Vector3[] meshVertices = this.meshFilter.mesh.vertices;

        int vertexIndex = 0;

        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                Vector3 vertex = meshVertices[vertexIndex];

                int colorIndex = y * 10 + x;

                meshVertices[vertexIndex] = new Vector3(vertex.x, tempArray[colorIndex] * 2, vertex.z);

                vertexIndex++;
            }
        }

        this.meshFilter.mesh.vertices = meshVertices;
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();

        this.meshCollider.sharedMesh = this.meshFilter.mesh;*/

    }

    // Update is called once per frame
    void Update()
    {

    }
}
