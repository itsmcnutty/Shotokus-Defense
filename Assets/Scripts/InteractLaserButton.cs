using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.Extras;
using UnityEngine.EventSystems;


public class InteractLaserButton : MonoBehaviour
{

    public GameObject rightHand;
    private bool selected;
    private SteamVR_LaserPointer laserPointer;

    public EnemyProducer enemyProducer;


    private void Awake()
    {
        laserPointer = rightHand.GetComponent<SteamVR_LaserPointer>();
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
     
        if (e.target.name == this.gameObject.name && selected==false)
        {
            selected = true;
            Debug.Log("pointer is inside this object" + e.target.name);
        }        
    }
    public void PointerOutside(object sender, PointerEventArgs e)
    {
     
        if (e.target.name == this.gameObject.name && selected == true)
        {
            selected = false;
            Debug.Log("pointer is outside this object" + e.target.name);
        }
    }
    
    public void OnPointerClick(object sender, PointerEventArgs e)
    {
//        IPointerClickHandler clickHandler = e.target.GetComponent<IPointerClickHandler>();
//        if (clickHandler == null)
//        {
//            return;
//        }
// 
// 
//        clickHandler.OnPointerClick(new PointerEventData(EventSystem.current));
        
        if (e.target.name == "Cube")
        {
            Debug.Log("Cube was exited");
        }
        else if (e.target.name == "Button")
        {
            Debug.Log("Button was clicked");
            enemyProducer.spawnEnemy(1);
        }
        
        
    }
    
    public bool get_selected_value()
    {
        return selected;
    }
    
}
