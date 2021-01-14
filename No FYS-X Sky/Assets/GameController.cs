using UnityEngine;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject player;
    private WorldBuilder worldBuilder;

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        worldBuilder = new WorldBuilder();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("PLAYERPOSITION : " + player.transform.position + "This is X: " + Mathf.FloorToInt(player.transform.position.x / 10) + " This is Z : " + Mathf.FloorToInt(player.transform.position.z / 10));
        Vector3 position = player.transform.position;
        worldBuilder.UnloadTiles(position);
        worldBuilder.UnloadRegions(position);
        worldBuilder.UnloadHouses();
        worldBuilder.LoadRegions(position);
        worldBuilder.LoadTiles(position);

    }

}