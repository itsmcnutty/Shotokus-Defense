using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]

public class RandomMesh : MonoBehaviour
{
    // Array of potential mesh choices
    public Mesh[] meshes;
    
    // Start is called before the first frame update
    void Start()
    {
        // Pick random Mesh from array
        int meshInd = Random.Range(0, meshes.Length);
        
        // Set to mesh used in SkinnedMeshRenderer
        gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh = meshes[meshInd];
    }
}
