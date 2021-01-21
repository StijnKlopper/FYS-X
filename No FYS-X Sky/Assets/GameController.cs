using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject player;
    private WorldBuilder worldBuilder;

    void Start()
    {
        worldBuilder = new WorldBuilder();
    }

    void Update()
    {
        Vector3 position = player.transform.position;
        worldBuilder.UnloadTiles(position);
        worldBuilder.UnloadRegions(position);
        worldBuilder.UnloadHouses();
        worldBuilder.LoadRegions(position);
        worldBuilder.LoadTiles(position);
    }

}