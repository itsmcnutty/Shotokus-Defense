using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class RandomMesh : MonoBehaviour
{
    // Array of potential mesh choices
    public Mesh[] meshes;
    // Array of corresponding material choices
    public Material[] mats;
    
    // Start is called before the first frame update
    void Start()
    {
        // Pick random Mesh from array
        int meshInd = Random.Range(0, meshes.Length);
        
        // Set to mesh used in SkinnedMeshRenderer and MeshCollider
        gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh = meshes[meshInd];
        gameObject.GetComponent<SkinnedMeshRenderer>().material = mats[meshInd];
        gameObject.GetComponent<MeshCollider>().sharedMesh = meshes[meshInd];
    }
}
