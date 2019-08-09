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
    private bool selected; 
    private SteamVR_LaserPointer laserPointer; // Laser pointer component from Steam VR
    private Button button; // this will be the button that the laser points to

    public EnemyProducer enemyProducer; // reference to the enemy producer to spawn enemies


    private void Awake()
    {
        laserPointer = rightHand.GetComponent<SteamVR_LaserPointer>();
        button = null;
//        laserPointer = gameObject.GetComponent<SteamVR_LaserPointer>();
    }


    // Start is called before the first frame update
    void Start()
    {
        laserPointer.PointerIn += PointerInside;
        laserPointer.PointerOut += PointerOutside;
        laserPointer.PointerClick += OnPointerClick;
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
            button = e.target.gameObject.GetComponent<Button>();
            button.Select();
            selected = true;
        }
    }
    
    
    public void PointerOutside(object sender, PointerEventArgs e)
    {
//        if (e.target.name == this.gameObject.name && selected == true)
//        {
//            selected = false;
//            Debug.Log("pointer is outside this object" + e.target.name);
//        }
        
        if (button != null)
        {
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
        if (selected && button != null)
        {
            button.onClick.Invoke();
        }

    }
    
    public bool get_selected_value()
    {
        return selected;
    }
    
}
