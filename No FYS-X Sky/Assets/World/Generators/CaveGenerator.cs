﻿using Assets.World.Generator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour, Generator
{
    [SerializeField]
    GameObject cavePrefab;

    private int tileOffset = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GenerateTile(Vector3 position)
    {
        Vector3 tilePosition = new Vector3(position.x + tileOffset, 20, position.z + tileOffset);
        GameObject tile = Instantiate(cavePrefab, tilePosition, Quaternion.Euler(0, 180, 0)) as GameObject;
        tile.transform.SetParent(this.transform);
        return tile;
    }
}
