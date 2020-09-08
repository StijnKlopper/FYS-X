using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;

namespace Assets.World
{
    class WorldBuilder : MonoBehaviour
    {

        private int seed;

        private TerrainGenerator terrainGenerator;

        private Vector3 currentChunkPosition;

        private Dictionary<Vector3, GameObject> tileDict = new Dictionary<Vector3, GameObject>();

        void Start()
        {

        }

        void Update()
        {

        }

        public void loadTiles(Vector3 position)
        {
            terrainGenerator = GameObject.Find("Level").GetComponent<TerrainGenerator>();

            // x-, x+, z-, z+
            int bounds = 50;
            int xMin = calcChunkCoord(position.x - bounds);
            int xMax = calcChunkCoord(position.x + bounds);
            int zMin = calcChunkCoord(position.z - bounds);
            int zMax = calcChunkCoord(position.z + bounds);

            for (int i = xMin; i < xMax; i += 10)
            {
                for (int j = zMin; j < zMax; j += 10)
                {
                    Vector3 newChunkPosition = new Vector3(calcChunkCoord(i), 0, calcChunkCoord(j));
                    if (!tileDict.ContainsKey(newChunkPosition))
                    {
                        tileDict.Add(newChunkPosition, terrainGenerator.GenerateTile(newChunkPosition));
                        currentChunkPosition = newChunkPosition;
                    }
                }
            }
        }

        public void unloadTiles(Vector3 position)
        {
            int bounds = 50;
            int xMin = calcChunkCoord(position.x - bounds);
            int xMax = calcChunkCoord(position.x + bounds);
            int zMin = calcChunkCoord(position.z - bounds);
            int zMax = calcChunkCoord(position.z + bounds);

            foreach (KeyValuePair<Vector3, GameObject> tile in tileDict.ToList())
            {
                if (tile.Key.x < xMin || tile.Key.x > xMax || tile.Key.z < zMin || tile.Key.z > zMax)
                {
                    Destroy(tile.Value);
                    tileDict.Remove(tile.Key);
                }
            }
        }

        private int calcChunkCoord(float coordinate)
        {
            return Mathf.FloorToInt(coordinate / 10) * 10;
        }

    }
}