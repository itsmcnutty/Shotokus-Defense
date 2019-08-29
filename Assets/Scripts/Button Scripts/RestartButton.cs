using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartButton : MonoBehaviour
{
    private GameObject gameControllerObject;
    private GameController gameController;
    private MenuUIController menuUiController;


    // Start is called before the first frame update
    void Start()
    {
        gameControllerObject = GameObject.FindWithTag("GameController");
        gameController = gameControllerObject.GetComponent<GameController>();
        menuUiController = GameObject.FindGameObjectWithTag("UIController").GetComponent<MenuUIController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RestartGame()
    {
        gameController.RestartGame();
        // if this object contains a gameover component then do not pause toggle
        if (GetComponentInParent<GameOverProperties>() != null)
        {
            return;
        }
        menuUiController.pauseToggle();
    }
    
}