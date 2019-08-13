using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartButton : MonoBehaviour
{
    private GameObject gameControllerObject;
    private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        gameControllerObject = GameObject.FindWithTag("GameController");
        gameController = gameControllerObject.GetComponent<GameController>();
        if (!gameController)
        {
            Debug.Log("ERROR: Coundnt find the instance GAMECONTROLLER");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RestartGame()
    {
        gameController.RestartGame();
    }
}
