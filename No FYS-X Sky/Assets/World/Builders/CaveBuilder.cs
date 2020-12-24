using System.Collections;
using UnityEngine;
using LibNoise.Generator;

using CielaSpike;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class CaveBuilder : MonoBehaviour
{

    RidgedMultifractal ridgedMultifractal;
    SafeMesh safeMesh;

    ConcurrentQueue<CaveThreadInfo<SafeMesh>> caveDataThreadInfoQueue = new ConcurrentQueue<CaveThreadInfo<SafeMesh>>();



/*    public void UpdateCaveMesh()
    {
        int height = 30;
        RequestCaveData(OnCaveDataReceived);
    }*/

    public void RequestCaveData(Action<SafeMesh> callback)
    {

        Vector2 offsets = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.z);

       
        ThreadStart threadStart = delegate
        {
            CaveDataThread(callback, offsets);
        };
        Thread thread = new Thread(threadStart);
        thread.IsBackground = true;
        thread.Priority = System.Threading.ThreadPriority.Lowest;



        thread.Start();
        //new Thread(threadStart).Start();
    }

    void CaveDataThread(Action<SafeMesh> callback, Vector2 offsets)
    {
        SafeMesh safeMesh = GenerateCaveMap(offsets, 30);

        caveDataThreadInfoQueue.Enqueue(new CaveThreadInfo<SafeMesh>(callback, safeMesh));
        
    }

    struct CaveThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public CaveThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

    }

    public void OnCaveDataReceived(SafeMesh safeMesh)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        mesh.Clear();
        mesh.vertices = safeMesh.Vertices;
        mesh.triangles = safeMesh.Triangles;

        //mesh.uv = GenerateUV.CalculateUVs(safeMesh.Vertices, 1);

        mesh.Optimize();
        mesh.RecalculateNormals();

        //Find out what this does
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;


    }

    public SafeMesh GenerateCaveMap(Vector2 offsets, int height)
    {
        int size = 11;
        // Gets added to coordinates, is a decimal to make sure it does not end up at an integer
        float addendum = 1000.17777f;

        // Coordinates are divided by scale, larger scale = larger/more spread out caves
        float scale = 20f;

        ridgedMultifractal = new RidgedMultifractal();
        ridgedMultifractal.OctaveCount = 3;

        float[, ,] caveMap = new float[size, height * 2, size];

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                // -5 to make the amount of rocks sticking out of the terrain lower
                int coordinateHeight = 0;
                int caveHeight = coordinateHeight + height;
                // Cave height make height dynamic based on heightmap[x,z]
                for (int y = 0; y < caveHeight; y++)
                {
                    if (y == 0)
                    {
                        caveMap[x, y, z] = 1;
                    }
                    else if (y <= 2)
                    {
                        double tempVal = ridgedMultifractal.GetValue((z + offsets.y + addendum) / scale, (y + addendum) / scale, (x + offsets.x + addendum) / scale);
                        int isCave = tempVal < 0 ? 0 : 1;
                        caveMap[x, y, z] = isCave;
                    }
                    else
                    {
                        double tempVal = ridgedMultifractal.GetValue((x + offsets.x + addendum) / scale, (y + addendum) / scale, (z + offsets.y + addendum) / scale);
                        int isCave = tempVal < 0.35 ? 0 : 1;
                        caveMap[x, y, z] = isCave;
                    }
                }
            }
        }

        MarchingCubes marchingCubes = new MarchingCubes();

        SafeMesh safeMesh = marchingCubes.BuildMesh(caveMap);
        return safeMesh;
    }



    public long FindPrimeNumber(int n)
    {
        int count = 0;
        long a = 2;
        while (count < n)
        {
            long b = 2;
            int prime = 1;// to check if found a prime
            while (b * b <= a)
            {
                if (a % b == 0)
                {
                    prime = 0;
                    break;
                }
                b++;
            }
            if (prime > 0)
            {
                count++;
            }
            a++;
        }
        return (--a);
    }


    void Update()
    {
        if (caveDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < caveDataThreadInfoQueue.Count; i++)
            {
                CaveThreadInfo<SafeMesh> result;

                if (caveDataThreadInfoQueue.TryDequeue(out result))
                {
                    result.callback(result.parameter);
                }
            }
        }
    }
}
