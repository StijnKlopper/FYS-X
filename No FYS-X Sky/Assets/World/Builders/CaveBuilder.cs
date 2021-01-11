using LibNoise.Generator;
using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

public class CaveBuilder : MonoBehaviour
{
    private RidgedMultifractal ridgedMultifractal;
    private ConcurrentQueue<CaveThreadInfo<SafeMesh>> caveDataThreadInfoQueue;
    public const int CAVE_DEPTH = 30;

    public void Start()
    {
        caveDataThreadInfoQueue = new ConcurrentQueue<CaveThreadInfo<SafeMesh>>();
        ridgedMultifractal = new RidgedMultifractal();
    }

    public void Instantiate(Vector3 position)
    {
        RequestCaveData(OnCaveDataReceived, position);
    }

    //Use Threadpool
    private void RequestCaveData(Action<SafeMesh> callback, Vector3 position)
    {
        ThreadPool.QueueUserWorkItem(delegate
        {
            CaveDataThread(callback, position);
        });
    }

    private void CaveDataThread(Action<SafeMesh> callback, Vector3 offsets)
    {
        SafeMesh safeMesh = GenerateCaveMap(offsets);

        caveDataThreadInfoQueue.Enqueue(new CaveThreadInfo<SafeMesh>(callback, safeMesh));
    }

    private void OnCaveDataReceived(SafeMesh safeMesh)
    {
        Tile currentTile = WorldBuilder.GetTile(safeMesh.position);
        //check if currentTile is not already unloaded
        if (currentTile != null) 
        {
            Mesh mesh = new Mesh();
            mesh.vertices = safeMesh.Vertices;
            mesh.triangles = safeMesh.Triangles;
            currentTile.Cave.MeshFilter.mesh = mesh;
            mesh.uv = GenerateUV.CalculateUVs(safeMesh.Vertices, 1);

            //mesh.Optimize();
            mesh.RecalculateNormals();

            //Find out what this does
            currentTile.Cave.MeshCollider.sharedMesh = null;
            currentTile.Cave.MeshCollider.sharedMesh = mesh;
            currentTile.Cave.GameObject.SetActive(true);
        }
    }

    private SafeMesh GenerateCaveMap(Vector3 offsets)
    {

        int size = WorldBuilder.CHUNK_SIZE + 1;
        Tile currentTile = WorldBuilder.GetTile(offsets);

        // Gets added to coordinates, is a decimal to make sure it does not end up at an integer
        float addendum = 1000.17777f;

        // Coordinates are divided by scale, larger scale = larger/more spread out caves
        float scale = 20f;
        ridgedMultifractal.OctaveCount = 3;

        float[,,] caveMap = new float[size, CAVE_DEPTH * 2, size];

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                // -5 to make the amount of rocks sticking out of the terrain lower
                //int coordinateHeight =  - 5;
                int coordinateHeight = Mathf.FloorToInt(currentTile.HeightMap[x, z]) - 5;
                int caveHeight = coordinateHeight + CAVE_DEPTH;
                // Cave height make height dynamic based on heightmap[x,z]
                for (int y = 0; y < caveHeight; y++)
                {
                    if (y == 0)
                    {
                        caveMap[x, y, z] = 1;
                    }
                    else if (y <= 2)
                    {
                        double tempVal = ridgedMultifractal.GetValue((z + offsets.z + addendum) / scale, (y + addendum) / scale, (x + offsets.x + addendum) / scale);
                        int isCave = tempVal < 0 ? 0 : 1;
                        caveMap[x, y, z] = isCave;
                    }
                    else
                    {
                        double tempVal = ridgedMultifractal.GetValue((x + offsets.x + addendum) / scale, (y + addendum) / scale, (z + offsets.z + addendum) / scale);
                        int isCave = tempVal < 0.35 ? 0 : 1;
                        caveMap[x, y, z] = isCave;
                    }
                }
            }
        }

        SafeMesh safeMesh = MarchingCubes.BuildMesh(caveMap);
        safeMesh.position = offsets;
        return safeMesh;
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

    void Update()
    {
        if (caveDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < caveDataThreadInfoQueue.Count; i++)
            {
                if (i < WorldBuilder.MAX_CHUNK_PER_FRAME)
                {
                    CaveThreadInfo<SafeMesh> result;

                    if (caveDataThreadInfoQueue.TryDequeue(out result))
                    {
                        result.callback(result.parameter);
                    }
                }
                else { break; }
            }
        }
    }
}
