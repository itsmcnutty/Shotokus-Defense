using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MenuUIController.Instance.pauseToggle();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        GameController.Instance.StartGame();
        MenuUIController.Instance.pauseToggle();
        GameObject.Find("Spawn Area").SetActive(false);
    }
    
}