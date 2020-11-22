using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInfo
{
    public enum TileType
    {
        Terrain,
        Cave,
        City
    }

    public Dictionary<TileType, GameObject> loadedTilesDict;

    public float[,] heightMap;

    public TileInfo()
    {
         loadedTilesDict = new Dictionary<TileType, GameObject>();
    }

}
