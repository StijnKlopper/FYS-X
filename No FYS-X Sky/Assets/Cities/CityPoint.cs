using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityPoint
{
    public Vector3 cubePosition;

    public List<Vector3> cityCoordinates; 

    public List<Vector3> invalidCityCoordinates;

    public List<GameObject> buildings;

    public CityPoint(Vector3 cubePosition)
    {
        this.cubePosition = cubePosition;
        this.cityCoordinates = new List<Vector3>();
        this.invalidCityCoordinates = new List<Vector3>();
        this.buildings = new List<GameObject>();
    }

    public int GetCountAllCityLocations()
    {
        return this.cityCoordinates.Count + this.invalidCityCoordinates.Count;
    }

    public void ReplaceOrAddCityPointCoordinate(bool validCoord, Vector3 coordinate)
    {
        // Remove the given coordinate from the lists
        if (cityCoordinates.Contains(coordinate))
        {
            cityCoordinates.Remove(coordinate);
        }
        if (invalidCityCoordinates.Contains(coordinate))
        {
            invalidCityCoordinates.Remove(coordinate);
        }

        // Add to correct location
        if (validCoord)
        {
            cityCoordinates.Add(coordinate);
        }
        else
        {
            invalidCityCoordinates.Add(coordinate);
        }
        
    }

}
