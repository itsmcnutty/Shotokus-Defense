using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameButton : MonoBehaviour
{
    public GameObject spawnArea;
    public GameObject tutorialArea;

    // Start is called before the first frame update
    void Start()
    {
        MenuUIController.Instance.ToggleLaser();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGameWithTutorial()
    {
        GameController.Instance.StartGameWithTutorial();
        spawnArea.SetActive(false);
        Time.timeScale = 0;
    }

    public void StartGameWithoutTutorial()
    {
        GameController.Instance.StartGameWithoutTutorial();
        MenuUIController.Instance.ToggleLaser();
        spawnArea.SetActive(false);
        tutorialArea.SetActive(false);
    }
    
}