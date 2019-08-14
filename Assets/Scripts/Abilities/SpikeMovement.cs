using UnityEngine;
using UnityEngine.AI;

public class SpikeMovement : MonoBehaviour
{
    private float speed;
    private Vector3 endPosition = new Vector3 ();
    // Start is called before the first frame update
    void Start ()
    {

    }

    // Update is called once per frame
    void Update ()
    {
        transform.position = Vector3.MoveTowards (transform.position, endPosition, speed);
        if (transform.position == endPosition)
        {
            Destroy (this, 5.0f);
        }
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
        PlayerAbility.MakeSpikeAvailable (gameObject);
    }
}