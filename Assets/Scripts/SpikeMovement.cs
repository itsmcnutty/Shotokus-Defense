using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeMovement : MonoBehaviour
{
    private float speed;
    private Vector3 endPosition = new Vector3(1,1,1);
    // Start is called before the first frame update
    void Start ()
    {

    }

    // Update is called once per frame
    void Update ()
    {
        transform.position = Vector3.MoveTowards (transform.position, endPosition, speed);
        if (transform.position == endPosition)
        {
            Destroy (gameObject, 5.0f);
        }
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }
    
    public void SetEndPosition(Vector3 endPosition)
    {
        this.endPosition = endPosition;
    }
}