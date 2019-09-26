using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceSourceController : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // Don't follow player head rotation, rotate to front always
        transform.rotation = Quaternion.identity;
    }
}
