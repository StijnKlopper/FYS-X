using Assets.World.Generator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;
using UnityEditor.UIElements;

public class CaveBuilder : MonoBehaviour
{

    RidgedMultifractal ridgedMultifractal;

    TerrainGenerator terrainGenerator;

    // Start is called before the first frame update
    void Start()
    {
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
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

        int height = 30;

        Tile tile = WorldBuilder.GetTile(new Vector3(-(offsets.x + 5), 0, -(offsets.y + 5)));
        float[,] heightMap = tile.heightMap;
        int[, ,] caveMap = new int[size, height * 2, size];

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                int coordinateHeight = Mathf.FloorToInt(heightMap[x, z]);
                int caveHeight = coordinateHeight + height;
                // Cave height make height dynamic based on heightmap[x,z]
                for (int y = 0; y < caveHeight; y++)
                {
                    double tempVal = ridgedMultifractal.GetValue((x + offsets.x + addendum) / scale, (y + addendum) / scale, (z + offsets.y + addendum) / scale);
                    int isCave = tempVal < 0.35 ? 0 : 1;
                    caveMap[x, y, z] = isCave;
                }
            }
        }

        CaveMeshGenerator caveMeshGen = GameObject.Find("Level").GetComponent<CaveMeshGenerator>();

        caveMeshGen.GenerateMesh(caveMap, 1, GetComponent<MeshFilter>());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
