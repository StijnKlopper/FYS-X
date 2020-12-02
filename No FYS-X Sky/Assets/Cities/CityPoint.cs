using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    public void CheckCoordinates(List<Vector3> houseCoordinates)
    {
        List<Vector3> PointsToRemove = new List<Vector3>();
        //To-do: Een andere manier vinden dan 3 loops te gebruiken om het uit de list halen misschien?
        foreach (var pos in houseCoordinates)
        {
            foreach (var citypos in cityCoordinates) 
            {
                if (citypos.x == pos.x && citypos.z == pos.z)
                {
                    //Prevent changing list while trough looping
                    PointsToRemove.Add(citypos);
                    continue;

                }
            }
          
        }
        RemoveCoordinates(PointsToRemove);

    }


    private void RemoveCoordinates(List<Vector3> coords)
    {
        foreach(var pos in coords)
        {
            Debug.Log("Removing");
            cityCoordinates.Remove(pos);
        }
        
    }

}
