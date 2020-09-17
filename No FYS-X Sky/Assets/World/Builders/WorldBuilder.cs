using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Xml.Schema;
using UnityEngine;

namespace Assets.World
{
    class WorldBuilder : MonoBehaviour
    {
        private TerrainGenerator terrainGenerator;

        private Dictionary<Vector3, GameObject> tileDict = new Dictionary<Vector3, GameObject>();

        void Start()
        {

        }

        void Update()
        {

        }

        public void LoadTiles(Vector3 position)
        {
            terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();

            // x-, x+, z-, z+
            int bounds = 100;
            int xMin = CalcChunkCoord(position.x - bounds);
            int xMax = CalcChunkCoord(position.x + bounds);
            int zMin = CalcChunkCoord(position.z - bounds);
            int zMax = CalcChunkCoord(position.z + bounds);

            for (int i = xMin; i < xMax; i += 10)
            {
                for (int j = zMin; j < zMax; j += 10)
                {
                    Vector3 newChunkPosition = new Vector3(CalcChunkCoord(i), 0, CalcChunkCoord(j));
                    if (!tileDict.ContainsKey(newChunkPosition))
                    {
                        GameObject tile = terrainGenerator.GenerateTile(newChunkPosition);
                        //Make the tiles a parent of the Level GameObject to have a clean hierarchy.
                        tile.transform.SetParent(terrainGenerator.transform);
                        tileDict.Add(newChunkPosition, tile);
                    }
                }
            }
        }

        public void UnloadTiles(Vector3 position)
        {
            int bounds = 100;
            int xMin = CalcChunkCoord(position.x - bounds);
            int xMax = CalcChunkCoord(position.x + bounds);
            int zMin = CalcChunkCoord(position.z - bounds);
            int zMax = CalcChunkCoord(position.z + bounds);

            foreach (KeyValuePair<Vector3, GameObject> tile in tileDict.ToList())
            {
                if (tile.Key.x < xMin || tile.Key.x > xMax || tile.Key.z < zMin || tile.Key.z > zMax)
                {
                    Destroy(tile.Value);
                    tileDict.Remove(tile.Key);
                }
            }
        }

        private int CalcChunkCoord(float coordinate)
        {
            return Mathf.FloorToInt(coordinate / 10) * 10;
        }
    }
}