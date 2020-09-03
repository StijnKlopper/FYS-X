using UnityEngine;

namespace Assets.World 
{
    class WorldBuilder : MonoBehaviour
    {
        
        private int seed;
       
        private LevelGenerator terrainGenerator;

        private Vector3 currentChunkPosition;

        void Start()
        {

            currentChunkPosition = new Vector3(0,0,0);
           
        }

        void Update()
        {
           
        }

        public void loadTiles(Vector3 position)
        {
            terrainGenerator = GameObject.Find("Level").GetComponent<LevelGenerator>();
            Vector3 newChunkPosition = new Vector3(Mathf.FloorToInt(position.x / 10) * 100, 0, Mathf.FloorToInt(position.z / 10) * 100);
            //if(currentChunkPosition.x != newChunkPosition.x && currentChunkPosition.z != newChunkPosition.z)
            //{
                
            //}


            terrainGenerator.GenerateTile(newChunkPosition);
        }


    }
}
