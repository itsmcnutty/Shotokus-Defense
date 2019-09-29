using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameButton : MonoBehaviour
{
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
    }

    public void StartGameWithoutTutorial()
    {
        GameController.Instance.StartGameWithoutTutorial();
        TutorialController.Instance.EndTutorial();
    }
    
}