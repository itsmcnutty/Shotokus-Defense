using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class navmeshDebug : MonoBehaviour
{
    LineRenderer lineRenderer; //to hold the line Renderer
//    var target : Transform; //to hold the transform of the target
    private NavMeshAgent agent; //to hold the agent of this gameObject

    public bool recalculate = false;
    public bool firstTime = false;

    void Start(){
        lineRenderer = GetComponent<LineRenderer>(); //get the line renderer
        agent = GetComponent<NavMeshAgent>(); //get the agent
//        getPath();
    }

    private void Update()
    {
        if (agent.hasPath)
        {
            if (!firstTime)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = new Vector3(agent.destination.x,2,agent.destination.z);

            }
            
            firstTime = true;
            recalculate = false;
            lineRenderer.positionCount = agent.path.corners.Length;
            lineRenderer.SetPositions(agent.path.corners);
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = false;
//            Debug.Log("my path is: " + agent.hasPath);
//            Debug.Log("my ispathstale is : "+ agent.isPathStale);
            Vector3 destination = agent.destination;
            if (firstTime)
            {
                agent.ResetPath();
                agent.SetDestination(new Vector3(0,0,0));
                firstTime = false;
            }
        }
    }
    
//    function getPath(){
//        line.SetPosition(0, transform.position); //set the line's origin
//
//        agent.SetDestination(target.position); //create the path
//        yield WaitForEndOfFrame(); //wait for the path to generate
//
//        DrawPath(agent.path);
//
//        agent.Stop();//add this if you don't want to move the agent
//    }
//
//    function DrawPath(path : NavMeshPath){
//        if(path.corners.Length < 2) //if the path has 1 or no corners, there is no need
//            return;
//
//        line.SetVertexCount(path.corners.Length); //set the array of positions to the amount of corners
//
//        for(var i = 1; i < path.corners.Length; i++){
//            line.SetPosition(i, path.corners[i]); //go through each corner and set that to the line renderer's position
//        }
//    }
}
