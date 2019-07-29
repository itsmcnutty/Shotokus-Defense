using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachSword : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        transform.parent = transform.parent.transform.Find("hand.R").transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
