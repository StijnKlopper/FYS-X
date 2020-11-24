﻿using Assets.World.Generator;
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

public class CaveBuilder : MonoBehaviour
{

    RidgedMultifractal ridgedMultifractal;

    TerrainGenerator terrainGenerator;

    // Start is called before the first frame update
    void Start()
    {
        terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();
        Vector2 offsets = new Vector2(-this.gameObject.transform.position.x, -this.gameObject.transform.position.z);
        StartCoroutine("UpdateMesh", offsets);
    }

    IEnumerator UpdateMesh(Vector2 offsets)
    {
        Mesh caveMesh = new Mesh();
        SafeMesh safemesh = null;
        safemesh = this.GenerateCaveMap(offsets, caveMesh);

        Mesh mesh = GetComponent<MeshFilter>().mesh;

        mesh.vertices = safemesh.Vertices;
        mesh.triangles = safemesh.Triangles;
        mesh.uv = new Vector2[(int)mesh.vertices.Length];
        mesh.Optimize();
        mesh.RecalculateNormals();
        //base.transform.localPosition = new Vector3((float)this.ChunkX, (float)this.ChunkY, (float)this.ChunkZ);
        /*        this.Filter.mesh = this.mesh;
                this.Collider.sharedMesh = null;
                this.Collider.sharedMesh = this.mesh;*/

        yield return null;
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

        //GameObject go = terrainGenerator.tileDict[new Vector3(offsets.x, 0, offsets.y)];
        float[,,] caveMap = new float[size, height, size];

        for (int x = 0; x < size; x++)
        {

            // Cave height make height dynamic based on heightmap[x,z]
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    double tempVal = ridgedMultifractal.GetValue((x + offsets.x + addendum) / scale, (y + addendum) / scale, (z + offsets.y + addendum) / scale);
                    int isCave = tempVal < 0.35 ? 0 : 1;
                    caveMap[x, y, z] = isCave;
                }
            }
        }
/*
        CaveMeshGenerator caveMeshGen = GameObject.Find("Level").GetComponent<CaveMeshGenerator>();

        caveMeshGen.GenerateMesh(caveMap, 1, GetComponent<MeshFilter>());*/

        SafeMesh safeMesh = MarchingCubes.BuildMesh(caveMap);

        
        /*caveMesh.vertices = safeMesh.Vertices;
        caveMesh.triangles = safeMesh.Triangles;*/
        /* caveMesh.uv = new Vector2[(int)caveMesh.vertices.Length];
         caveMesh.Optimize();
         caveMesh.RecalculateNormals();*/

        //GetComponent<MeshFilter>().mesh = caveMesh;

        return safeMesh;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
