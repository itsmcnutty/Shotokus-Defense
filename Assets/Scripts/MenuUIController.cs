using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class MenuUIController : MonoBehaviour
{
    public SteamVR_Input_Sources rightHandInput;
    public SteamVR_Input_Sources leftHandInput;
    public SteamVR_Action_Boolean pauseAction;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // todo make this toggleable, so if not in menu access it
        // todo - otherwise, close it
        if (pausePress())
        {
            Debug.Log("Im being PaUsEd");
        }
    }

    public bool pausePress()
    {
        return pausePressLeft() || pausePressRight();
    }
    
    public bool pausePressRight()
    {
        return pauseAction.GetState(rightHandInput);
    }    
    
    public bool pausePressLeft()
    {
        return pauseAction.GetState(leftHandInput);
    }
}
