using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallProperties : MonoBehaviour
{

    private bool collisionDetected;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, 30.0f);
    }
}
