using UnityEngine;

public class RockProperties : MonoBehaviour
{
    private ParticleSystem destroyRockParticles;

    private static float rockLifetime = 5.0f;
    // Start is called before the first frame update
    void Start ()
    {
        Invoke ("DestroyRock", rockLifetime);
    }

    // Update is called once per frame
    void Update ()
    { }

    private void OnDestroy ()
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
        ParticleSystem particleSystem = Instantiate(destroyRockParticles);
        particleSystem.transform.position = gameObject.transform.position;

        UnityEngine.ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.scale = gameObject.transform.localScale;

        UnityEngine.ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.rateOverTimeMultiplier = gameObject.transform.localScale.x * emission.rateOverTimeMultiplier;

        gameObject.transform.position = new Vector3 (0, -10, 0);
        gameObject.SetActive(false);
        Rocks.MakeRockAvailable (gameObject);
        Destroy(this);
    }

    public static float GetRockLifetime()
    {
        return rockLifetime;
    }

}