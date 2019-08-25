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
        SkinnedMeshRenderer mesh = this.GetComponentInChildren<SkinnedMeshRenderer> ();
        float meshY= mesh.bounds.size.y;
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
        Vector3 startPosB = wallPos + transform.forward * 2; // position for navmesh link start point
        Vector3 startPosF = wallPos - transform.forward * 2; // position for navmesh link start point

        // todo uncomment later
        wallProperties = GetComponent<WallProperties>();
        if (wallProperties == null)
        {
            Debug.Log("No wall properties");
            
        }
        
        float height = wallProperties.wallHeightPercent;
        float wallHeightaboveGround = height * wallMaxHeight;
        float floorHeight = transform.position.y - wallHeightaboveGround;
        startPosB.y = floorHeight;
        startPosF.y = floorHeight;
        
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
