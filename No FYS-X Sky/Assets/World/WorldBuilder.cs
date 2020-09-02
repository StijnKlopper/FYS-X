using System.Collections.Generic;
using UnityEngine;

namespace Assets.World 
{
    class WorldBuilder : MonoBehaviour
    {
        public GameObject plane;
        private int seed;
        private List<GameObject> planeList; 
        private void GenerateWorld()
        {

        }

        void Start()
        {
            plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.position = new Vector3(0, 0, 0);
            planeList = new List<GameObject>();
            planeList.Add(plane);
        }

        void Update()
        {

        }

        private void GenerateWorld(Vector3 position) 
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.position = position;
            planeList.Add(plane);
        }

    }
}
