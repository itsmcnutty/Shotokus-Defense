using UnityEngine;

public class RockProperties : MonoBehaviour
{
    public ParticleSystem destroyRockParticles;

    private static float rockLifetime = 5.0f;
    private bool collidedWithEnemy;
    private bool collidedWithNonEnemy;
    // Start is called before the first frame update
    void Start ()
    {
    }

    // Update is called once per frame
    void Update ()
    { }

    public void StartDestructionTimer()
    {
        Invoke ("DestroyRock", rockLifetime);
    }

    public void CancelDestructionTimer ()
    {
        CancelInvoke ("DestroyRock");
    }

    public static void CreateComponent(GameObject rock, ParticleSystem destroyRockParticles)
    {
        RockProperties properties = rock.AddComponent<RockProperties>();
        properties.destroyRockParticles = destroyRockParticles;
    }

    public void DestroyRock ()
    {
        // Plays the particle effect when the rock is destroyed
        ParticleSystem particleSystem = Instantiate(destroyRockParticles);
        particleSystem.transform.position = gameObject.transform.position;

        // Sets size and number of particles to reflect the size of the rock
        UnityEngine.ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.scale = gameObject.transform.localScale;

        UnityEngine.ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.rateOverTimeMultiplier = gameObject.transform.localScale.x * emission.rateOverTimeMultiplier;

        // Moves the rock out of the map and readds it to the stash of rocks
        gameObject.transform.position = new Vector3 (0, -10, 0);
        gameObject.SetActive(false);
        Rocks.MakeRockAvailable (gameObject);
    }

    public static float GetRockLifetime()
    {
        return rockLifetime;
    }

    public bool CollidedWithEnemy()
    {
        return collidedWithEnemy;
    }

    public bool CollidedWithNonEnemy()
    {
        return collidedWithNonEnemy;
    }

    private void OnCollisionEnter(Collision other) {
        if(other.gameObject.layer == 9)
        {
            collidedWithEnemy = true;
            collidedWithNonEnemy = false;
            return;
        }
        collidedWithNonEnemy = true;
        collidedWithEnemy = false;
    }

}