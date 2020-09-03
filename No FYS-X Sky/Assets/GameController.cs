using Assets.World;
using System;
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
        
        //Debug.Log("PLAYERPOSITION : " + player.transform.position +  "This is X: " + Mathf.FloorToInt(player.transform.position.x/10)+ " This is Z : " + Mathf.FloorToInt(player.transform.position.z / 10));
        worldBuilder.loadTiles(player.transform.position);

    }


}
 