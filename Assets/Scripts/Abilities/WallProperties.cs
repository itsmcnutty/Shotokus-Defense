using UnityEngine;

public class WallProperties : MonoBehaviour
{
    public float wallMoveSpeed = 0f;
    public Vector3 direction = new Vector3();
    private float wallLifetime = 30.0f;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating ("MoveWall", 0, 0.01f);
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, wallLifetime);
    }

    void OnDestroy()
    {
    }

    private void MoveWall()
    {
        if(wallMoveSpeed != 0)
        {
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, gameObject.transform.position + (direction * wallMoveSpeed), 1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag != "Ground" && other.gameObject.layer != 9 && other.gameObject.layer != 11)
        {
            CancelInvoke("MoveWall");
        }
    }
}
