using UnityEngine;
using Valve.VR.InteractionSystem;

public class RockProperties : MonoBehaviour
{
    public ParticleSystem destroyRockParticles;
    
    [Header("Audio")]
    public AudioClip[] rockHitSolid;
    public AudioClip[] rockHitFoliage;
    public PhysicMaterial foliageMaterial;
    
    private SoundPlayOneshot randomizedAudioSource;
    
    // Audio source for playing sounds out of rock itself
    private AudioSource audioSource;

    private static float rockLifetime = 5.0f;
    private bool collidedWithEnemy;
    private bool collidedWithNonEnemy;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyRock", rockLifetime);
        audioSource = GetComponent<AudioSource>();
        randomizedAudioSource = GetComponent<SoundPlayOneshot>();
        
        // Set loop to false because rock has left hand and can't be regrown
        audioSource.loop = false;
    }

    // Update is called once per frame
    void Update() { }

    public void StartDestructionTimer()
    {
        Invoke ("DestroyRock", rockLifetime);
    }

    public void CancelDestructionTimer()
    {
        CancelInvoke("DestroyRock");
    }

    public static void CreateComponent(GameObject rock, ParticleSystem destroyRockParticles, AudioClip[] rockHitSolid,
        AudioClip[] rockHitFoliage, PhysicMaterial foliageMaterial)
    {
        RockProperties properties = rock.AddComponent<RockProperties>();
        properties.destroyRockParticles = destroyRockParticles;
        properties.rockHitSolid = rockHitSolid;
        properties.rockHitFoliage = rockHitFoliage;
        properties.foliageMaterial = foliageMaterial;
        
        // Get the child which holds the audio component that plays the spawn sound and play it
        properties.transform.GetChild(0).gameObject.GetComponent<SoundPlayOneshot>().Play();
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
        Rocks.MakeRockAvailable(gameObject);
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
        
        if (other.collider.material == foliageMaterial)
        {
            randomizedAudioSource.waveFiles = rockHitFoliage;
        }
        else
        {
            randomizedAudioSource.waveFiles = rockHitSolid;
        }
        
        randomizedAudioSource.Play();
    }

}