using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatInteractable : MonoBehaviour
{
    public float bobHeight = .04f;
    private float bottomPosY;
    private float topPosY;

    public float rotationDegree = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        bottomPosY = transform.position.y - bobHeight;
        topPosY = transform.position.y + bobHeight;
    }

    // Update is called once per frame
    void Update()
    {
        float bobPercent = Mathf.Sin(Time.time) * 0.5f + 0.5f;
        float currentHeight = Mathf.Lerp(bottomPosY, topPosY, bobPercent);
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
        
        float rotateDegreesRight = Mathf.Sin(Time.time) * 0.5f;
        float rotateDegreesForward = Mathf.Sin(Time.time + 180) * 0.5f;
        transform.RotateAround(transform.position, Vector3.right, rotateDegreesRight);
        transform.RotateAround(transform.position, Vector3.forward, rotateDegreesForward);

        // float rightRotatePercent = Mathf.Sin(Time.time) * 0.5f + 0.5f;
        // float forwardRotatePercent = Mathf.Cos(Time.time) * 0.5f + 0.5f;

        // Vector3 axisRight = Vector3.Lerp(Vector3.zero, Vector3.right, rightRotatePercent);
        // Vector3 axisForward = Vector3.Lerp(Vector3.zero, Vector3.forward, forwardRotatePercent);

        // transform.RotateAround(transform.position, axisRight, rotationDegree);
        //transform.RotateAround(transform.position, axisForward, rotationDegree);
    }
}
