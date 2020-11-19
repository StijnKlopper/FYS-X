using Assets.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject player;
    WorldBuilder worldBuilder;

    void Start()
    {
        worldBuilder = new WorldBuilder();
    }

    private void FixedUpdate()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        //Debug.Log("PLAYERPOSITION : " + player.transform.position + "This is X: " + Mathf.FloorToInt(player.transform.position.x / 10) + " This is Z : " + Mathf.FloorToInt(player.transform.position.z / 10));
        Vector3 position = player.transform.position;
        worldBuilder.LoadRegions(position);
        worldBuilder.LoadTiles(position);
        worldBuilder.UnloadTiles(position);
        worldBuilder.UnloadRegions(position);
        //worldBuilder.UnloadCityPoints(position);
    }

}