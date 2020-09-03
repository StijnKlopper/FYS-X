using System.Collections.Generic;
using UnityEngine;

namespace Assets.World 
{
    class WorldBuilder : MonoBehaviour
    {
        
        private int seed;
       
        private LevelGenerator terrainGenerator;

        private Vector3 currentChunkPosition;

        private Dictionary<Vector3, GameObject> tileDict = new Dictionary<Vector3, GameObject>();

        void Start()
        {
            startTile(new Vector3(0, 0, 0));
        }

        void Update()
        {
           
        }

        public void loadTiles(Vector3 position)
        {
            terrainGenerator = GameObject.Find("Level").GetComponent<LevelGenerator>();


            Vector3 newChunkPosition = new Vector3(Mathf.FloorToInt(position.x / 10) * 10, 0, Mathf.FloorToInt(position.z / 10) * 10);
            if((currentChunkPosition.x != newChunkPosition.x || currentChunkPosition.z != newChunkPosition.z) && tileDict.ContainsKey(newChunkPosition))
            {
                tileDict.Add(newChunkPosition, terrainGenerator.GenerateTile(newChunkPosition));
                currentChunkPosition = newChunkPosition;
            }
        }

        private void startTile(Vector3 position)
        {
            currentChunkPosition = position;
            terrainGenerator = GameObject.Find("Level").GetComponent<LevelGenerator>();
            tileDict.Add(position, terrainGenerator.GenerateTile(position));
        }
    }
}
