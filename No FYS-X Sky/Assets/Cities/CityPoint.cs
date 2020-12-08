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

    public void ReplaceOrAddCityPointCoordinateList(bool validCoord, List<Vector3> coordinates)
    {
        foreach (Vector3 coordinate in coordinates)
        {
            ReplaceOrAddCityPointCoordinate(validCoord, coordinate);
        }

    }

    public void CheckCoordinates(List<Vector3> houseCoordinates)
    {
        List<Vector3> PointsToRemove = new List<Vector3>();
        //To-do: Een andere manier vinden dan 3 loops te gebruiken om het uit de list halen misschien?
        foreach (Vector3 pos in houseCoordinates)
        {
            foreach (Vector3 citypos in cityCoordinates) 
            {
                if (citypos.x == pos.x && citypos.z == pos.z)
                {
                    // Prevent changing list while trough looping
                    PointsToRemove.Add(citypos);
                    continue;
                }
            }
          
        }
        RemoveCoordinates(PointsToRemove);

    }

    private void RemoveCoordinates(List<Vector3> coords)
    {
        foreach(Vector3 pos in coords)
        {
            cityCoordinates.Remove(pos);
        }
        
    }

}
