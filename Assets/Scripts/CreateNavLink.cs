using UnityEngine;
using UnityEngine.AI;


public class CreateNavLink : MonoBehaviour
{
    // NavMesh Links Prefabs
    public Transform nmLinkPrefab; 
    
    // references to navMeshLink Front and Back objects and scripts
    private Transform navMeshLinkObjB;
    private Transform navMeshLinkObjF;
    private NavMeshLink navMeshLinkB; 
    private NavMeshLink navMeshLinkF;

    private WallProperties wallProperties;
    
    // Start is called before the first frame update
    private void Awake()
    {
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void createLinks()
    {
        // get position of wall
        Vector3 wallPos = transform.position;
        // get size of wall mesh
        MeshRenderer mesh = this.GetComponentInChildren<MeshRenderer> ();
        float meshX = mesh.bounds.size.x;
        float meshZ = mesh.bounds.size.z;

        // Set transform and rotation to 0 for navmesh link
        Vector3 pos = new Vector3((float) 0, (float) 0, (float) 0);
        Quaternion rot = Quaternion.Euler((float)0, (float)0, (float)0);
        
        // Instantiate navLink Prefab both Front and Back
        navMeshLinkObjB = Instantiate(nmLinkPrefab.transform, pos, rot);
        navMeshLinkObjF = Instantiate(nmLinkPrefab.transform, pos, rot);
        navMeshLinkB = navMeshLinkObjB.GetComponent<NavMeshLink>();
        navMeshLinkF = navMeshLinkObjF.GetComponent<NavMeshLink>();

        // Calculate positions for nav mesh links
        Vector3 startPosB = wallPos + transform.forward; // position for navmesh link start point
        Vector3 startPosF = wallPos - transform.forward; // position for navmesh link start point

        wallProperties = GetComponent<WallProperties>();

        // Store the height of wall on front and back
        float wallHeightF = -10;
        float wallHeightB = -10;
        // shoot raycast from point at startPos + the height y of wall in wallPos
        // This will allow us to know the height from the wall to the floor
        RaycastHit hitF;
        RaycastHit hitB;
        Vector3 originFront = new Vector3(startPosF.x, wallPos.y, startPosF.z);
        Vector3 originBack = new Vector3(startPosB.x, wallPos.y, startPosB.z);
        // todo implement for backside too
        Vector3 rayDirection = Vector3.down; // shoot laser downwards
        
        // set where the ray is coming from and its direction
        Ray visionRayF = new Ray(originFront, rayDirection);
        Ray visionRayB = new Ray(originBack, rayDirection);
        
        // if we hit the ground, set the heigh of wall equal to difference between total heigh of wall and distance to floor
        if (Physics.Raycast(visionRayF, out hitF)) 
        {
            if (hitF.collider.CompareTag("Ground"))
            {
                wallHeightF = hitF.distance;
            }
        }
        if (Physics.Raycast(visionRayB, out hitB)) 
        {
            if (hitB.collider.CompareTag("Ground"))
            {
                wallHeightB = hitB.distance;
            }
        }
        
        float heightF = wallHeightF; 
        float heightB = wallHeightB; 
        float floorHeightF = transform.position.y - heightF;
        float floorHeightB = transform.position.y - heightB;
        startPosF.y = floorHeightF;
        startPosB.y = floorHeightB;
        
        // calculate vector that goes foward and back of the wall
        Vector3 directionWidthF = new Vector3(startPosF.x, wallPos.y,  startPosF.z) - wallPos;
        directionWidthF = directionWidthF.normalized;
        Vector3 directionWidthB = new Vector3(startPosB.x, wallPos.y,  startPosB.z) - wallPos;
        directionWidthB = directionWidthF.normalized;
        
        // add the direction to the transform vector to obtain the points at the corners
        Vector3 wallPosF = wallPos - directionWidthF * 0.2f;
        Vector3 wallPosB = wallPos + directionWidthB * 0.2f;

        navMeshLinkB.startPoint = startPosB;
        navMeshLinkF.startPoint = startPosF;
        navMeshLinkB.endPoint = wallPosF;
        navMeshLinkF.endPoint = wallPosB;
        navMeshLinkB.width = meshZ / 3; // todo testing smaller link
        navMeshLinkF.width = meshZ / 3;
        
        navMeshLinkB.UpdateLink();
        navMeshLinkF.UpdateLink();
        navMeshLinkObjB.SetParent(this.transform);
        navMeshLinkObjF.SetParent(this.transform);
    }
}
