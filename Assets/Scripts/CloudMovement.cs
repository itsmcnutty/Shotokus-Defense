using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    public Vector3 startLoc;
    public Vector3 endLoc;
    public float moveSpeed = 10;
    public float sqrMetersBeforeFade = 100;
    private Renderer objectRenderer;
    private Color objectColor;
    // Start is called before the first frame update
    void Start()
    {
        objectRenderer = gameObject.GetComponent<Renderer>();
        objectColor = objectRenderer.sharedMaterial.color;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, endLoc, moveSpeed);

        Vector3 distanceToStartVector = startLoc - transform.position;
        float distanceToStart = Vector3.SqrMagnitude(distanceToStartVector);
        if(distanceToStart < sqrMetersBeforeFade)
        {
            objectColor.a = Mathf.Lerp(0, 1, distanceToStart / sqrMetersBeforeFade);
            objectRenderer.sharedMaterial.color = objectColor;
        }

        Vector3 distanceToEndVector = endLoc - transform.position;
        float distanceToEnd = Vector3.SqrMagnitude(distanceToEndVector);
        if(distanceToEnd < sqrMetersBeforeFade)
        {
            objectColor.a = Mathf.Lerp(0, 1, distanceToEnd / sqrMetersBeforeFade);
            objectRenderer.sharedMaterial.color = objectColor;
        }

        if(transform.position == endLoc)
        {
            transform.position = startLoc;
        }
    }
}
