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
    MeshFilter floorMeshFilter;

    [SerializeField]
    MeshFilter wallMeshFilter;

    RidgedMultifractal ridgedMultifractal;

    // Start is called before the first frame update
    void Start()
    {
        GenerateCaveMap();
    }


    public void GenerateCaveMap()
    {
        int size = 11;

        // Gets added to coordinates, is a decimal to make sure it does not end up at an integer
        float addendum = 1000.17777f;

        // Coordinates are divided by scale, larger scale = larger/more spread out caves
        float scale = 20f;

        Vector2 offsets = new Vector2(-this.gameObject.transform.position.x, -this.gameObject.transform.position.z);

        ridgedMultifractal = new RidgedMultifractal();
        ridgedMultifractal.OctaveCount = 3;

        int[,] caveMap = new int[size, size];

        for (int height = 0; height < 20; height++) {

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    double tempVal = ridgedMultifractal.GetValue((x + offsets.x + addendum) / scale, height, (y + offsets.y + addendum) / scale);
                    int isCave = tempVal < 0.35 ? 0 : 1;
                    caveMap[x, y] = isCave;
                }
            }
        }

        CaveMeshGenerator caveMeshGen = GameObject.Find("Level").GetComponent<CaveMeshGenerator>();
        
        caveMeshGen.GenerateMesh(caveMap, 1, this.floorMeshFilter, this.wallMeshFilter);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
