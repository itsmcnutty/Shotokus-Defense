using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameButton : MonoBehaviour
{
    public GameObject spawnArea;

    // Start is called before the first frame update
    void Start()
    {
        MenuUIController.Instance.ToggleLaser();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        GameController.Instance.StartGame();
        MenuUIController.Instance.ToggleLaser();
        spawnArea.SetActive(false);
    }
    
}