using Assets.World.Generator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;
using UnityEditor.UIElements;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

using CielaSpike;

public class CaveBuilder : MonoBehaviour
{

    RidgedMultifractal ridgedMultifractal;

    TerrainGenerator terrainGenerator;

    CaveGenerator caveGenerator;

    SafeMesh safemesh;

    GameObject caveFloor;

    // Start is called before the first frame update
    void Start()
    {
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        caveGenerator = GameObject.Find("Level").GetComponent<CaveGenerator>();
        StartCoroutine(updateCaveMesh());
    }

    public IEnumerator updateCaveMesh() {
        int height = 30;

        Task task;
        Mesh caveMesh = new Mesh();
        Vector2 offsets = new Vector2(-this.gameObject.transform.position.x, -this.gameObject.transform.position.z);
        this.StartCoroutineAsync(generateCaveMap(caveMesh, offsets, height), out task);
        yield return StartCoroutine(task.Wait());

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        mesh.vertices = safemesh.Vertices;
        mesh.triangles = safemesh.Triangles;

        mesh.Optimize();
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
        caveFloor = caveGenerator.GenerateCaveFloor(new Vector3(this.gameObject.transform.position.x - 5, -height, this.gameObject.transform.position.z - 5));
    }

    IEnumerator generateCaveMap(Mesh caveMesh, Vector2 offsets, int height)
    {
        safemesh = this.GenerateCaveMap(offsets, caveMesh, height);
        yield return safemesh;
    }


    public SafeMesh GenerateCaveMap(Vector2 offsets, Mesh caveMesh, int height)
    {
        int size = 11;

        // Gets added to coordinates, is a decimal to make sure it does not end up at an integer
        float addendum = 1000.17777f;

        // Coordinates are divided by scale, larger scale = larger/more spread out caves
        float scale = 20f;

        ridgedMultifractal = new RidgedMultifractal();
        ridgedMultifractal.OctaveCount = 3;

        

        Tile tile = WorldBuilder.GetTile(new Vector3(-(offsets.x ), 0, -(offsets.y )));


        float[,] heightMap = tile.heightMap;

        float[, ,] caveMap = new float[size, height * 2, size];

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

        SafeMesh safeMesh = MarchingCubes.BuildMesh(caveMap);


        return safeMesh;
    }

    private void OnDestroy()
    {
        Destroy(caveFloor);
    }
}
