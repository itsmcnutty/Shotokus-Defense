using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallParentCollision : MonoBehaviour
{

    private CallParentCollision parentComponent;
    
    // Start is called before the first frame update
    void Start()
    {
        parentComponent = transform.parent.gameObject.GetComponent<CallParentCollision>();
    }

    // Called when a collision event occurs to this game object
    private void OnCollisionEnter(Collision other)
    {
        // Parent must also have a CallParentCollision component to pass collision data along
        if (parentComponent != null)
        {
            // Pass collision data up to parent
            parentComponent.OnCollisionEnterChild(gameObject, other);
        }
    }

    // Called when a collision event occurs to a child with the CallParentCollision component
    protected virtual void OnCollisionEnterChild(GameObject child, Collision other)
    {
        // Parent must also have a CallParentCollision component to pass collision data along
        if (parentComponent != null)
        {
            // Pass collision data up to parent
            parentComponent.OnCollisionEnterChild(child, other);
        }
    }
}
