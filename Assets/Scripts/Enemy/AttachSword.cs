using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachSword : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        Transform handBone = transform.parent.transform.Find(
            "free3dmodel_skeleton/hips/abdomen/abdomen2/chest/shoulder.R/upper_arm.R/forearm.R/hand.R");
        
        // Make sword child of the hand bone
        transform.parent = handBone.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
