using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellBehavior : MonoBehaviour
{
    public GameObject lantern;
    private MazeMaker maze;
    private GameplayAudio gameAudio;

    void Awake()
    {
        lantern.SetActive(false);
        GameObject manager = GameObject.FindWithTag("MazeManager");
        maze = manager.GetComponent<MazeMaker>();
        gameAudio = manager.GetComponent<GameplayAudio>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!lantern.activeSelf)
        {
            lantern.SetActive(true);
            maze.CellIlluminated(transform.position);
            gameAudio.PlayNextNote();
        }
    }

    
}
