using System.Collections;
using UnityEngine;
using LibNoise.Generator;

using CielaSpike;

public class CaveBuilder : MonoBehaviour
{

    RidgedMultifractal ridgedMultifractal;
    SafeMesh safemesh;
    float[,] heightmap;

    public void initiate(float[,] heightmap) {
        this.heightmap = heightmap;
        StartCoroutine(UpdateCaveMesh());
    }

    public IEnumerator UpdateCaveMesh()
    {
        int height = 30;

        Task task;
        Mesh caveMesh = new Mesh();
        Vector2 offsets = new Vector2(-this.gameObject.transform.position.x, -this.gameObject.transform.position.z);
        this.StartCoroutineAsync(generateCaveMap(caveMesh, offsets, height), out task);
        yield return StartCoroutine(task.Wait());

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        mesh.Clear();
        mesh.vertices = safemesh.Vertices;
        mesh.triangles = safemesh.Triangles;

        mesh.uv = GenerateUV.CalculateUVs(safemesh.Vertices, 1);

        mesh.Optimize();
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;

        yield return null;
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

        //Tile tile = WorldBuilder.GetTile(new Vector3(-(offsets.x ), 0, -(offsets.y )));

        //Debug.Log(tile.heightMap);*/

        //float[,] heightMap = tile.heightMap;

        float[, ,] caveMap = new float[size, height * 2, size];

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                int coordinateHeight = Mathf.FloorToInt(heightmap[x, z]);
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

        SafeMesh safeMesh = MarchingCubes.BuildMesh(caveMap);
        return safeMesh;
    }
}
