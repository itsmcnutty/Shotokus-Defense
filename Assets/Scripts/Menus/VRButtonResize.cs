using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(RectTransform))]
public class VRButtonResize : MonoBehaviour
{
    
    private BoxCollider boxCollider;
    private RectTransform rectTransform;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnEnable()
    {
        ResizeCollider();
    }

    private void OnValidate()
    {
        ResizeCollider();
    }

    private void ResizeCollider()
    {
        rectTransform = GetComponent<RectTransform>();
        boxCollider = GetComponent<BoxCollider>();
        
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        boxCollider.size = rectTransform.sizeDelta;
    }
}

