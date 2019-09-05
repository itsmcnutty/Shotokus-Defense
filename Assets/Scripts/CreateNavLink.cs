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

    public void createLinks(float wallMaxHeight)
    {
        // get position of wall
        Vector3 wallPos = transform.position;
        // get size of wall mesh
        MeshRenderer mesh = this.GetComponentInChildren<MeshRenderer> ();
        float meshX = mesh.bounds.size.x;
        
        // Set transform and rotation to 0 for navmesh link
        Vector3 pos = new Vector3((float) 0, (float) 0, (float) 0);
        Quaternion rot = Quaternion.Euler((float)0, (float)0, (float)0);
        
        // Instantiate navLink Prefab both Front and Back
        navMeshLinkObjB = Instantiate(nmLinkPrefab.transform, pos, rot);
        navMeshLinkObjF = Instantiate(nmLinkPrefab.transform, pos, rot);
        navMeshLinkB = navMeshLinkObjB.GetComponent<NavMeshLink>();
        navMeshLinkF = navMeshLinkObjF.GetComponent<NavMeshLink>();

        // Calculate positions for nav mesh links
        Vector3 startPosB = wallPos + transform.forward * 1.5f; // position for navmesh link start point
        Vector3 startPosF = wallPos - transform.forward * 1.5f; // position for navmesh link start point

        wallProperties = GetComponent<WallProperties>();
        if (wallProperties == null)
        {
            Debug.Log("No wall properties");
        }

        // Store the height of wall on front
        float wallHeightF = -10;
        // shoot raycast from point at startPos + the height y of wall in wallPos
        // This will allow us to know the height from the wall to the floor
        RaycastHit hit;
        Vector3 originFront = new Vector3(startPosF.x, wallPos.y, startPosF.z);
        // todo implement for backside too
        Vector3 rayDirection = startPosF - originFront; // todo this might shoot from the feet
        
        // set where the ray is coming from and its direction
        Ray visionRay = new Ray(originFront, rayDirection);

        Debug.DrawRay(originFront, rayDirection, Color.red);
        
        // if we hit the ground ... print out height that would be distance of ray
        if (Physics.Raycast(visionRay, out hit)) 
        {
            if (hit.collider.CompareTag("Ground"))
            {
                wallHeightF = hit.distance;
                // we can hit the player, so shoot
                Debug.Log("We hit the ground!!");
                Debug.Log("Height is " + wallHeightF);
            }
        }
        
        float height = wallHeightF; 
        float floorHeight = transform.position.y - height;
        startPosB.y = floorHeight;
        startPosF.y = floorHeight;
        
//        float height = wallProperties.WallHeightPercent;
//        float wallHeightaboveGround = height * wallMaxHeight;
//        float floorHeight = transform.position.y - wallHeightaboveGround;
//        startPosB.y = floorHeight;
//        startPosF.y = floorHeight;
        
        navMeshLinkB.startPoint = startPosB;
        navMeshLinkF.startPoint = startPosF;
        navMeshLinkB.endPoint = wallPos;
        navMeshLinkF.endPoint = wallPos;
        navMeshLinkB.width = meshX;
        navMeshLinkF.width = meshX;
        
        navMeshLinkB.UpdateLink();
        navMeshLinkF.UpdateLink();
        navMeshLinkObjB.SetParent(this.transform);
        navMeshLinkObjF.SetParent(this.transform);
    }
}
