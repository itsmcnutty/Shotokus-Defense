using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class DebugFunctionality : MonoBehaviour
{
    private GameObject vrCamera; // referenced as our player, contains player scripts
    private GameController gameController;
    private MenuUIController menuUiController;
    private Text enemiesLeft; // text in pause menu
    private Text totalEnemies; // text in pause menu

    // Start is called before the first frame update
    void Start()
    {
        vrCamera = GameObject.FindGameObjectWithTag("MainCamera");
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        menuUiController = GameObject.FindGameObjectWithTag("UIController").GetComponent<MenuUIController>();
//        enemiesLeft = transform.Find("Canvas/EnemiesLeft").GetComponent<Text>();
//        totalEnemies = transform.Find("Canvas/TotalEnemies").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
//        int subs = gameController.numOfEnemiesPerWave - gameController.enemiesDestroyed;
//        enemiesLeft.text = subs.ToString();
//        totalEnemies.text = gameController.numOfEnemiesPerWave.ToString();
    }

    public void takeDamage()
    {
//        Debug.Log("Taking damage");
        menuUiController.pauseToggle();
        vrCamera.GetComponent<PlayerHealth>().TakeDamage(100);
    }

    public void teleport()
    {
        gameController.Teleport(false);
        menuUiController.pauseToggle();
    }

    public void ResumeGame()
    {
        menuUiController.pauseToggle();
    }

    public void RestartGame()
    {
        menuUiController.pauseToggle();
        gameController.RestartGame();
    }

}