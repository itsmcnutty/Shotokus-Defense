using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;


public class CreateNavLink : MonoBehaviour
{
    // NavMesh Links Prefabs
    public Transform nmLinkPrefab; 
    
    // references to navMeshSurface
    private GameObject navMeshObject;
    private NavMeshSurface navMeshSurface;
    
    // references to navMeshLink Front and Back objects and scripts
    private Transform navMeshLinkObjB;
    private Transform navMeshLinkObjF;
    private NavMeshLink navMeshLinkB; 
    private NavMeshLink navMeshLinkF;

    private WallProperties wallProperties;
    
    // Start is called before the first frame update
    private void Awake()
    {
        navMeshObject = GameObject.FindGameObjectWithTag("NavMesh Light");
        navMeshSurface = navMeshObject.GetComponent<NavMeshSurface>();
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void createLinks(float width)
    {
                // get position of wall
        Vector3 wallPos = transform.position;
        
        // Set transform and rotation to 0 for navmesh link
        Vector3 pos = new Vector3((float) 0, (float) 0, (float) 0);
        Quaternion rot = Quaternion.Euler((float)0, (float)0, (float)0);
        
        // Instantiate navLink Prefab both Front and Back
        navMeshLinkObjB = Instantiate(nmLinkPrefab.transform, pos, rot);
        navMeshLinkObjF = Instantiate(nmLinkPrefab.transform, pos, rot);
        navMeshLinkB = navMeshLinkObjB.GetComponent<NavMeshLink>();
        navMeshLinkF = navMeshLinkObjF.GetComponent<NavMeshLink>();

        // Calculate positions for nav mesh links
        Vector3 startPosB = wallPos + transform.forward * 2; // position for navmesh link start point
        Vector3 startPosF = wallPos - transform.forward * 2; // position for navmesh link start point

        // todo uncomment later
        wallProperties = GetComponent<WallProperties>();
        if (wallProperties == null)
        {
            Debug.Log("No wall properties");
            
        }
        
        SkinnedMeshRenderer mesh = this.GetComponentInChildren<SkinnedMeshRenderer> ();
        var meshY= mesh.bounds.size.y;
        
        
        // todo THIS SHOULD NOT BE HARD CODED
        // TODO BUG : wall properties are found but the heigh percent returned is 0
        float height = wallProperties.wallHeightPercent;
        float wallHeightaboveGround = height * meshY / 3f;
        float floorHeight = transform.position.y - wallHeightaboveGround;
        startPosB.y = floorHeight;
        startPosF.y = floorHeight;
//        startPosB.y = 0;
//        startPosF.y = 0;
        
        navMeshLinkB.startPoint = startPosB;
        navMeshLinkF.startPoint = startPosF;
        
        navMeshLinkB.endPoint = wallPos;
        navMeshLinkF.endPoint = wallPos;

        // todo THIS SHOULD NOT BE HARD CODED
//        navMeshLinkB.width = transform.localScale.z;
//        navMeshLinkF.width = transform.localScale.z;
//        navMeshLinkB.width = 1;
//        navMeshLinkF.width = 1;
        navMeshLinkB.width = width;
        navMeshLinkF.width = width;
        
        navMeshLinkB.UpdateLink();
        navMeshLinkF.UpdateLink();
        navMeshLinkObjB.SetParent(this.transform);
        navMeshLinkObjF.SetParent(this.transform);
    }
}
