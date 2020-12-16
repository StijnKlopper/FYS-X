using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour, Generator
{
    private int[] randomNumbers;

    public int seed;

    [Range(0, 10)]
    public int minWidth;
    public int maxWidth;

    [Range(0, 10)]
    public int minDepth;
    public int maxDepth;

    [Range(0, 10)]
    public int minFloors;
    public int maxFloors;

    [SerializeField]
    List<GameObject> floors;

    [SerializeField]
    List<GameObject> walls;

    private void Start()
    {
        // TODO: Get seed

        System.Random random = new System.Random(seed);
        this.randomNumbers = new int[100];

        for (int i = 0; i < this.randomNumbers.Length; i++)
        {
            this.randomNumbers[i] = random.Next(0, 10);
        }
    }

    public void Generate()
    {
        Debug.Log("Generate");

    }

    public void RandomValues()
    {
        // TODO: Make fake values
        Debug.Log("TODO");
    }

    private void OnValidate()
    {
        // TODO: Validate input values
    }
}
