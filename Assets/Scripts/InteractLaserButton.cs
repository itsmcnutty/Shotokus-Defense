using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.Extras;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class InteractLaserButton : MonoBehaviour
{

    public GameObject rightHand; // right hand VR
    public GameObject leftHand; // right hand VR
    private bool selected; 
    private SteamVR_LaserPointer laserPointerR; // Laser pointer for Right hand
    private SteamVR_LaserPointer laserPointerL; // Laser pointer for Left hand
    private Button button; // this will be the button that the laser points to
    
    private bool isEnabled; // true if lasers are enable, false otherwise

    private ControllerArc rightArc;
    private ControllerArc leftArc;

    private void Awake()
    {
        laserPointerR = rightHand.GetComponent<SteamVR_LaserPointer>();
        laserPointerL = leftHand.GetComponent<SteamVR_LaserPointer>();
        button = null;

        rightArc = rightHand.GetComponentInChildren<ControllerArc>();
        leftArc = leftHand.GetComponentInChildren<ControllerArc>();

        if (rightArc != null)
        {
            Debug.Log("we found the ARC!!");
        }
        else
        {
            Debug.Log("EERRORRR NO ARRRC");
        }

    }


    // Start is called before the first frame update
    void Start()
    {
        laserPointerR.PointerIn += PointerInside;
        laserPointerR.PointerOut += PointerOutside;
        laserPointerR.PointerClick += OnPointerClick;
        
        laserPointerL.PointerIn += PointerInside;
        laserPointerL.PointerOut += PointerOutside;
        laserPointerL.PointerClick += OnPointerClick;

//        isEnabled = false;
//        laserPointerL.enabled = false;
//        laserPointerR.enabled = false;
        selected = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void PointerInside(object sender, PointerEventArgs e)
    {
        if (e.target.gameObject.GetComponent<Button>() != null && button == null)
        {
            Debug.Log("Inside the button");
            button = e.target.gameObject.GetComponent<Button>();
            button.Select();
            selected = true;
        }
        else
        {
            Debug.Log("THis is not a button");
        }
    }
    
    
    public void PointerOutside(object sender, PointerEventArgs e)
    {
        if (button != null && selected)
        {
            Debug.Log("Outside the button");
            selected = false;
            // todo what is this for??
//            myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
            button = null;
        }
    }
    
    public void OnPointerClick(object sender, PointerEventArgs e)
    {
        Debug.Log("Clicking somewhere");
        if (selected && button != null)
        {
            Debug.Log("Clicking inside the button!");
            button.onClick.Invoke();
        }

    }

    // This function enables or disables (toggles on or off) the steamVR laser pointer component
    public void toggleLaser()
    {
        if (isEnabled)
        {
            Debug.Log("disabling");
            isEnabled = false;
//            laserPointerL.enabled = false;
//            laserPointerL.active = false;
//            laserPointerR.enabled = false;
//            laserPointerR.active = false;
            rightArc.enabled = true;
            leftArc.enabled = true;

        }
        else
        {
            Debug.Log("enabling");
            // laser is not enabled, so enable it
//            isEnabled = true;
//            laserPointerL.enabled = true;
//            laserPointerL.active = true;
//            laserPointerR.enabled = true;
//            laserPointerR.active = true;
            rightArc.enabled = false;
            leftArc.enabled = false;


        }
    }
    
    public bool get_selected_value()
    {
        return selected;
    }
    
}
