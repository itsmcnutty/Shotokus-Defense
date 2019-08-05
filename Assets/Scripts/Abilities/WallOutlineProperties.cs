using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallOutlineProperties : MonoBehaviour
{

    private bool collisionDetected;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public bool CollisionDetected()
    {
        return collisionDetected;
    }

    private void OnTriggerEnter(Collider other)
    {
        collisionDetected = true;
        Debug.Log("entered");
    }

    private void OnTriggerExit(Collider other)
    {
        collisionDetected = false;
        Debug.Log("exit");
    }
}
