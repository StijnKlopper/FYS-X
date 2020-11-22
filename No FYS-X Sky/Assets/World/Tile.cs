using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public enum TileType
    {
        Terrain,
        Cave,
        City
    }

    public Dictionary<TileType, GameObject> loadedTilesDict;

    public float[,] heightMap;

    public Tile()
    {
         loadedTilesDict = new Dictionary<TileType, GameObject>();
    }

}
