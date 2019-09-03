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

    // Start is called before the first frame update
    void Start()
    {
        obstacle = GetComponent<NavMeshObstacle>();
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 nextPos = Vector3.MoveTowards(transform.position, endPosition, speed);
        if (!colliding)
        {
            transform.position = nextPos;
        }
        else
        {
            colliding = false;
        }

        if (transform.position.y >= endPosition.y * .9 && !particleEffectPlayed)
        {
            particleEffectPlayed = true;

            ParticleSystem earthParticleSystem = Instantiate(createSpikeEarthParticles);
            earthParticleSystem.transform.position = startPos;
            earthParticleSystem.transform.localScale = transform.localScale;

            UnityEngine.ParticleSystem.VelocityOverLifetimeModule velocityModule = earthParticleSystem.velocityOverLifetime;
            velocityModule.speedModifierMultiplier = speed;
        }

        if (transform.position == endPosition)
        {

            obstacle.enabled = true;
            Destroy(this, 2.0f);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            colliding = true;
            other.rigidbody.velocity = speed / Time.deltaTime * Vector3.Normalize(endPosition - transform.position);
        }
    }

    public static void CreateComponent(GameObject spike, float speed, Vector3 endPosition, ParticleSystem createSpikeEarthParticles, ParticleSystem destroySpikeParticles)
    {
        SpikeMovement spikeMovement = spike.AddComponent<SpikeMovement>();
        spikeMovement.speed = speed;
        spikeMovement.endPosition = endPosition;
        spikeMovement.destroySpikeParticles = destroySpikeParticles;
        spikeMovement.createSpikeEarthParticles = createSpikeEarthParticles;
    }

    private void OnDestroy()
    {
        ParticleSystem particleSystem = Instantiate(destroySpikeParticles);
        particleSystem.transform.position = transform.position;
        particleSystem.transform.rotation = transform.rotation;

        UnityEngine.ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.scale = transform.localScale;

        gameObject.transform.position = new Vector3(0, -10, 0);
        gameObject.SetActive(false);
        SpikeQuicksand.MakeSpikeAvailable(gameObject);
    }
}