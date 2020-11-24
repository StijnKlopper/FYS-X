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

    SafeMesh safemesh;

    // Start is called before the first frame update
    void Start()
    {
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        StartCoroutine(updateCaveMesh());
    }

    public IEnumerator updateCaveMesh() {
        Task task;
        Mesh caveMesh = new Mesh();
        Vector2 offsets = new Vector2(-this.gameObject.transform.position.x, -this.gameObject.transform.position.z);
        this.StartCoroutineAsync(generateCaveMap(caveMesh, offsets), out task);
        yield return StartCoroutine(task.Wait());

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        mesh.vertices = safemesh.Vertices;
        mesh.triangles = safemesh.Triangles;
        mesh.uv = new Vector2[(int)mesh.vertices.Length];
        mesh.Optimize();
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    IEnumerator generateCaveMap(Mesh caveMesh, Vector2 offsets)
    {
        safemesh = this.GenerateCaveMap(offsets, caveMesh);
        yield return safemesh;
    }


    public SafeMesh GenerateCaveMap(Vector2 offsets, Mesh caveMesh)
    {
        int size = 11;

        // Gets added to coordinates, is a decimal to make sure it does not end up at an integer
        float addendum = 1000.17777f;

        // Coordinates are divided by scale, larger scale = larger/more spread out caves
        float scale = 20f;

        ridgedMultifractal = new RidgedMultifractal();
        ridgedMultifractal.OctaveCount = 3;

        int height = 20;

        Tile tile = WorldBuilder.GetTile(new Vector3(-(offsets.x + 5), 0, -(offsets.y + 5)));


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

    // Update is called once per frame
    void Update()
    {

    }
}
