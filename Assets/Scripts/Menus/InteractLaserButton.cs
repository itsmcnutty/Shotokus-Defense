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

    [Header("Audio Sources")]
    public AudioSource menuClick;
    public AudioSource menuMisclick;

    private ControllerArc rightArc;
    private ControllerArc leftArc;
    private GameObject rightArcObject;
    
    private Hand rightHandComp;
    private Hand leftHandComp;
    private GameObject rightHandHeld;
    private GameObject leftHandHeld;
    
    private AudioSource audioSource;

    // event system to keep track of selected buttons
    private EventSystem eventSystem;
    private string lastButtonName; // name of gameobject selected by laserpointer


    private void Awake()
    {
        // Instantiate stuff
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
        
        audioSource = GetComponent<AudioSource>();
        eventSystem = GameController.Instance.GetComponent<EventSystem>();
    }


    // Start is called before the first frame update
    void Start()
    {
        lastButtonName = ""; // so its not null
        
        laserPointerR.PointerIn += PointerInside;
        laserPointerR.PointerOut += PointerOutside;
        laserPointerR.PointerClick += OnPointerClick;
        
        laserPointerL.PointerIn += PointerInside;
        laserPointerL.PointerOut += PointerOutside;
        laserPointerL.PointerClick += OnPointerClick;
    }
    
    // todo button color select 6491FF

    private void Update()
    {
//        if (eventSystem.currentSelectedGameObject != null)
//        {
//            string selectedObj = eventSystem.currentSelectedGameObject.name;
//            Debug.Log(selectedObj);
//        }

    }

    public void PointerInside(object sender, PointerEventArgs e)
    {
        if (e.target.gameObject.GetComponent<Button>() != null)
        {
            Debug.Log("inside button + " + e.target.gameObject.name);
            button = e.target.gameObject.GetComponent<Button>();
            button.Select();
            lastButtonName = e.target.gameObject.name;
        }
    }
    
    public void PointerOutside(object sender, PointerEventArgs e)
    {
        button = null;
        if (eventSystem.currentSelectedGameObject != null)
        {
//            string selectedObj = eventSystem.currentSelectedGameObject.name;
            eventSystem.SetSelectedGameObject(null);
        }
        
//        if (lastButtonName == e.target.gameObject.name)
//        {
//            Debug.Log("Pointer out: lastname button = current target name + " + e.target.gameObject.name);
//            eventSystem.SetSelectedGameObject(null);
//            button = null;
//        }
//        
//        if (e.target.gameObject.GetComponent<Button>() == null)
//        {
//            Debug.Log("Pointer out: Hitting something without a button + " + e.target.gameObject.name);
//            if (button != null)
//            {
////                eventSystem.SetSelectedGameObject(null);
////                button = null;
//            }
//        }
//        else
//        {
//            Debug.Log("Pointer out: hitting something with a button + " + e.target.gameObject.name);
////            button.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
//
//            if (eventSystem.currentSelectedGameObject != null)
//            {
//                string selectedObj = eventSystem.currentSelectedGameObject.name;
//                eventSystem.SetSelectedGameObject(null);
//            }
//            button = null;
//        }
//        
    }
    
    public void OnPointerClick(object sender, PointerEventArgs e)
    {
        if (e.target.gameObject.GetComponent<Button>() != null && button != null)
        {
            Debug.Log("clicking inside button");
            button.onClick.Invoke();
        }
        else
        {
            // Play misclick sound
            menuMisclick.Play();
        }
        

//        if (selected && button != null)
//        {
//            // Play click sound
//            menuClick.Play();
//            button.onClick.Invoke();
//        }
//        else
//        {
//            // Play misclick sound
//            menuMisclick.Play();
//        }

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
            laserPointerL.active = false; // todo debugging make this true if you want both lasers back
            
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
