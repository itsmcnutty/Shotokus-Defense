using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockCrash : MonoBehaviour
{

    private bool crashed = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        crashed = true;
    }
}
