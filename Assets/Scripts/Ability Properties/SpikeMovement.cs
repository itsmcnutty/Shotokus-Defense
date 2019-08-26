using System;
using UnityEngine;
using UnityEngine.AI;

public class SpikeMovement : MonoBehaviour
{
    private float speed;
    private Vector3 endPosition;
    private NavMeshObstacle obstacle;
    private bool colliding = false;
    
    // Start is called before the first frame update
    void Start ()
    {
        obstacle = GetComponent<NavMeshObstacle>();
    }

    // Update is called once per frame
    void Update ()
    {
        Vector3 nextPos = Vector3.MoveTowards (transform.position, endPosition, speed);
        if (!colliding)
        {
            transform.position = nextPos;
        }
        else
        {
            colliding = false;
        }
        
        if (transform.position == endPosition)
        {
            obstacle.enabled = true;
            Destroy (this, 2.0f);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            colliding = true;
            other.rigidbody.velocity =  speed / Time.deltaTime * Vector3.Normalize(endPosition - transform.position);
        }
    }

    private void OnCollisionExit()
    {
        //colliding = false;
    }

    public static void CreateComponent (GameObject spike, float speed, Vector3 endPosition)
    {
        SpikeMovement spikeMovement = spike.AddComponent<SpikeMovement> ();
        spikeMovement.speed = speed;
        spikeMovement.endPosition = endPosition;
    }

    private void OnDestroy ()
    {
        gameObject.transform.position = new Vector3 (0, -10, 0);
        gameObject.SetActive(false);
        SpikeQuicksand.MakeSpikeAvailable (gameObject);
    }
}