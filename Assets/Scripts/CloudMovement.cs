using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    public Vector2 startLocation;
    public Vector2 endLocation;
    public float moveSpeed = 10;
    public float sqrMetersBeforeFade = 100;
    private Vector3 startLocation3D;
    private Vector3 endLocation3D;
    private float initialAlpha;
    private Renderer objectRenderer;
    private Color objectColor;
    private MaterialPropertyBlock block;
    // Start is called before the first frame update
    void Start()
    {
        objectRenderer = gameObject.GetComponent<Renderer>();
        objectColor = objectRenderer.sharedMaterial.color;
        initialAlpha = objectColor.a;
        block = new MaterialPropertyBlock();

        startLocation3D = new Vector3(startLocation.x, transform.position.y, startLocation.y);
        endLocation3D = new Vector3(endLocation.x, transform.position.y, endLocation.y);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, endLocation, moveSpeed);

        Vector3 distanceToStartVector = startLocation3D - transform.position;
        float distanceToStart = Vector3.SqrMagnitude(distanceToStartVector);
        if(distanceToStart < sqrMetersBeforeFade)
        {
            objectColor.a = Mathf.Lerp(0, initialAlpha, distanceToStart / sqrMetersBeforeFade);
            block.SetColor("_BaseColor", objectColor);
            objectRenderer.SetPropertyBlock(block);
        }

        Vector3 distanceToEndVector = endLocation3D - transform.position;
        float distanceToEnd = Vector3.SqrMagnitude(distanceToEndVector);
        if(distanceToEnd < sqrMetersBeforeFade)
        {
            objectColor.a = Mathf.Lerp(0, initialAlpha, distanceToEnd / sqrMetersBeforeFade);
            block.SetColor("_BaseColor", objectColor);
            objectRenderer.SetPropertyBlock(block);
        }

        if(transform.position == endLocation3D)
        {
            transform.position = startLocation3D;
        }
    }
}
