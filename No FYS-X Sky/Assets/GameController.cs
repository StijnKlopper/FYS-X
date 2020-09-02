using Assets.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject player;
    WorldBuilder worldBuilder;
    public float Seerange = 100f;
    void Start()
    {
        worldBuilder = new WorldBuilder();

    }

    // Update is called once per frame
    void Update()
    {
        //if(player.transform.position)
    }


}
 