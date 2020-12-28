using System.Collections;
using UnityEngine;
using LibNoise.Generator;

using CielaSpike;

public class CaveBuilder : MonoBehaviour
{

    private RidgedMultifractal ridgedMultifractal;
    private SafeMesh safeMesh;
    private float[,] heightMap;

    public void Instantiate(float[,] heightMap) {
        this.heightMap = heightMap;
        StartCoroutine(UpdateCaveMesh());
    }

    private IEnumerator UpdateCaveMesh()
    {
        int height = 30;

        Mesh caveMesh = new Mesh();
        Vector2 offsets = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.z);
        this.StartCoroutineAsync(GenerateCaveMap(caveMesh, offsets, height), out Task task);
        yield return StartCoroutine(task.Wait());

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        mesh.Clear();
        mesh.vertices = safeMesh.Vertices;
        mesh.triangles = safeMesh.Triangles;

        mesh.uv = GenerateUV.CalculateUVs(safeMesh.Vertices, 1);

        mesh.Optimize();
        mesh.RecalculateNormals();

        //Find out what this does
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;

        yield return null;
    }

    private IEnumerator GenerateCaveMap(Mesh caveMesh, Vector2 offsets, int height)
    {
        safeMesh = this.GenerateCaveMap(offsets, caveMesh, height);
        yield return safeMesh;
    }

    private SafeMesh GenerateCaveMap(Vector2 offsets, Mesh caveMesh, int height)
    {
        int size = WorldBuilder.CHUNK_SIZE + 1;

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
                int coordinateHeight = Mathf.FloorToInt(heightMap[x, z]) - 5;
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
