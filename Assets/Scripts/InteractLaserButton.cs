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
    

    private void Awake()
    {
        laserPointerR = rightHand.GetComponent<SteamVR_LaserPointer>();
        laserPointerL = leftHand.GetComponent<SteamVR_LaserPointer>();
        button = null;
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
        
        selected = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void PointerInside(object sender, PointerEventArgs e)
    {
//        if (e.target.name == this.gameObject.name && selected==false)
//        {
//            selected = true;
//            Debug.Log("pointer is inside this object" + e.target.name);
//        }        
        
        if (e.target.gameObject.GetComponent<Button>() != null && button == null)
        {
            //Debug.Log("Inside the button");
            button = e.target.gameObject.GetComponent<Button>();
//            button.Select();
            selected = true;
        }
        else
        {
            //Debug.Log("THis is not a button");
        }
    }
    
    
    public void PointerOutside(object sender, PointerEventArgs e)
    {
//        if (e.target.name == this.gameObject.name && selected == true)
//        {
//            selected = false;
//            Debug.Log("pointer is outside this object" + e.target.name);
//        }
        
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
//        if (e.target.name == "Button")
//        {
//            Debug.Log("Button was clicked");
//            enemyProducer.spawnEnemy(1);
//        }
        Debug.Log("Clicking somewhere");
        if (selected && button != null)
        {
            Debug.Log("Clicking inside the button!");
            button.onClick.Invoke();
        }

    }
    
    public bool get_selected_value()
    {
        return selected;
    }
    
}
