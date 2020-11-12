﻿using Assets.World.Generator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;
using UnityEditor.UIElements;

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

        int height = 20;

        int[, ,] caveMap = new int[size, height, size];

        for (int x = 0; x < size; x++) {

            // Cave height
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

        CaveMeshGeneratorTemp caveMeshGen = GameObject.Find("Level").GetComponent<CaveMeshGeneratorTemp>();
        
        caveMeshGen.GenerateMesh(caveMap, 1, this.floorMeshFilter, this.wallMeshFilter);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
