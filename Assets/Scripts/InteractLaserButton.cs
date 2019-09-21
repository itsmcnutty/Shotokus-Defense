using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.Extras;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;



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
    private GameObject rightArcObject;
    
    private Hand rightHandComp;
    private Hand leftHandComp;
    private GameObject rightHandHeld;
    private GameObject leftHandHeld;

    private void Awake()
    {
        laserPointerR = rightHand.GetComponent<SteamVR_LaserPointer>();
        laserPointerL = leftHand.GetComponent<SteamVR_LaserPointer>();
        button = null;
        
        rightArc = rightHand.GetComponentInChildren<ControllerArc>();
        leftArc = leftHand.GetComponentInChildren<ControllerArc>();
        
        rightHandComp = rightHand.GetComponent<Hand>();
        leftHandComp = leftHand.GetComponent<Hand>();
        
        isEnabled = false;
        selected = false;
        laserPointerL.active = false;
        laserPointerR.active = false;
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void PointerInside(object sender, PointerEventArgs e)
    {
        if (e.target.gameObject.GetComponent<Button>() != null && button == null)
        {
            //Debug.Log("Inside the button");
            button = e.target.gameObject.GetComponent<Button>();
            button.Select();
            selected = true;
        }
        else
        {
            //Debug.Log("THis is not a button");
        }
    }
    
    public void PointerOutside(object sender, PointerEventArgs e)
    {
        if (button != null && selected)
        {
//            Debug.Log("Outside the button");
            selected = false;
            // todo what is this for??
//            myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
            button = null;
        }
    }
    
    public void OnPointerClick(object sender, PointerEventArgs e)
    {
//        Debug.Log("Clicking somewhere");
        if (selected && button != null)
        {
//            Debug.Log("Clicking inside the button!");
            button.onClick.Invoke();
        }

    }

    // This function enables or disables (toggles on or off) the steamVR laser pointer component
    // if rock is being held, it disappears and appears again when menu is unpaused if trigger is still hold
    public void toggleLaser()
    {
        if (isEnabled)
        {
            isEnabled = false;
            laserPointerL.active = false;
            laserPointerR.active = false;

            // enable abilities & controller arc
            rightArc.setCanUseAbility(true);
            leftArc.setCanUseAbility(true);
            rightArc.enabled = true;
            leftArc.enabled = true;
            
            // make rocks appear in player's hand if they hold a rock
            if (rightHandHeld != null)
            {
                rightHandComp.AttachObject(rightHandHeld, GrabTypes.Scripted);
                rightHandHeld.GetComponent<SkinnedMeshRenderer>().enabled = true;
                rightHandHeld.transform.position = rightHandComp.objectAttachmentPoint.position;
                rightHandHeld = null;
            }
            if (leftHandHeld != null)
            {
                leftHandComp.AttachObject(leftHandHeld, GrabTypes.Scripted);
                leftHandHeld.GetComponent<SkinnedMeshRenderer>().enabled = true;
                leftHandHeld.transform.position = leftHandComp.objectAttachmentPoint.position;
                leftHandHeld = null;
            }        
        }
        else
        {
            // laser is not enabled, so enable it
            isEnabled = true;
            laserPointerR.active = true;
            laserPointerL.active = true;
            
            // disable abilities & controller arc
            rightArc.setCanUseAbility(false);
            leftArc.setCanUseAbility(false);
            rightArc.HidePointer();
            leftArc.HidePointer();
            rightArc.ClearPointerHitObject();
            leftArc.ClearPointerHitObject();
            rightArc.enabled = false;
            leftArc.enabled = false;

            // make rocks disappear from player's hand if they pause the game
            if (rightHandComp.currentAttachedObject != null)
            {
                rightHandHeld = rightHandComp.currentAttachedObject;
                rightHandComp.DetachObject(rightHandHeld);
                rightHandHeld.GetComponent<SkinnedMeshRenderer>().enabled = false;
            }
            if (leftHandComp.currentAttachedObject != null)
            {
                leftHandHeld = leftHandComp.currentAttachedObject;
                leftHandComp.DetachObject(leftHandHeld);
                leftHandHeld.GetComponent<SkinnedMeshRenderer>().enabled = false;
            }
        }
    }
    

}
