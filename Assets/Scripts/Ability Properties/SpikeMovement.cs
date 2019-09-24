using UnityEngine;
using UnityEngine.AI;

public class SpikeMovement : MonoBehaviour
{
    private ParticleSystem createSpikeEarthParticles;
    private ParticleSystem destroySpikeParticles;
    private float speed;
    private Vector3 endPosition;
    private NavMeshObstacle obstacle;
    private bool colliding = false;

    private Vector3 startPos;
    private bool particleEffectPlayed = false;
    private GameObject parentObject;

    // Start is called before the first frame update
    void Start()
    {
        obstacle = GetComponent<NavMeshObstacle>();
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Moves the spike towards the endPosition unless it hits something
        Vector3 nextPos = Vector3.MoveTowards(parentObject.transform.position, endPosition, speed);
        if (!colliding)
        {
            parentObject.transform.position = nextPos;
        }
        else
        {
            colliding = false;
        }

        if (parentObject.transform.position.y >= endPosition.y * .9 && !particleEffectPlayed)
        {
            // Plays particle effect once after the given percentage of the way through the raising
            particleEffectPlayed = true;

            // Plays particle effect at the position of the object with its size
            ParticleSystem earthParticleSystem = Instantiate(createSpikeEarthParticles);
            earthParticleSystem.transform.position = startPos;
            earthParticleSystem.transform.localScale = transform.localScale;

            // The speed if the particle effect depends on the speed of the play's hands
            UnityEngine.ParticleSystem.VelocityOverLifetimeModule velocityModule = earthParticleSystem.velocityOverLifetime;
            velocityModule.speedModifierMultiplier = speed;
        }

        if (parentObject.transform.position == endPosition)
        {
            // Once it reaches the peak, destroy the object
            obstacle.enabled = true;
            Destroy(this, 2.0f);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        // Enemies that get hit by the spike get a velocity added to them to make them fly away
        if (other.gameObject.layer == 9)
        {
            colliding = true;
            other.gameObject.GetComponentInParent<RagdollController>().StartRagdoll();
        }
    }

    public static void CreateComponent(GameObject spike, float speed, Vector3 endPosition, ParticleSystem createSpikeEarthParticles, ParticleSystem destroySpikeParticles)
    {
        SpikeMovement spikeMovement = spike.transform.GetChild(0).gameObject.AddComponent<SpikeMovement>();
        spikeMovement.parentObject = spike;
        spikeMovement.speed = speed;
        spikeMovement.endPosition = endPosition;
        spikeMovement.destroySpikeParticles = destroySpikeParticles;
        spikeMovement.createSpikeEarthParticles = createSpikeEarthParticles;
    }

    private void OnDestroy()
    {
        // Disable obstacle for when this spike is re-created later
        obstacle.enabled = false;
        
        // Plays the particle effect on death
        ParticleSystem particleSystem = Instantiate(destroySpikeParticles);
        particleSystem.transform.position = transform.position;
        particleSystem.transform.rotation = transform.rotation;

        // Changes the shape to match the size of the spike
        UnityEngine.ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.scale = transform.localScale;

        // Move the spike, disable it, and readd it to the stash
        gameObject.transform.position = new Vector3(0, -10, 0);
        gameObject.SetActive(false);
        SpikeQuicksand.MakeSpikeAvailable(gameObject);
    }
}