using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class RockProperties : MonoBehaviour
{
    public float rockPunchTimeDelay = 0.25f;
    public ParticleSystem destroyRockParticles;
    
    [Header("Audio")]
    public AudioClip[] rockHitSolid;
    public AudioClip[] rockHitFoliage;
    public PhysicMaterial foliageMaterial;
    
    private SoundPlayOneshot randomizedAudioSource;
    
    // Audio source for playing sounds out of rock itself
    private AudioSource audioSource;
    private AudioSource activeRockAudioSource;
    private Rigidbody rockRigidbody;
    private SkinnedMeshRenderer rockMesh;

    private static float rockLifetime = 5.0f;
    private bool collidedWithEnemy;
    private bool collidedWithNonEnemy;

    private List<int> enemiesHit = new List<int>();
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        randomizedAudioSource = GetComponent<SoundPlayOneshot>();
        rockRigidbody = GetComponent<Rigidbody>();
        rockMesh = GetComponent<SkinnedMeshRenderer>();
        activeRockAudioSource = GetComponent<AudioSource>();
        
        // Set loop to false because rock has left hand and can't be regrown
        audioSource.loop = false;
    }

    // Update is called once per frame
    void Update() { }

    public void StartDestructionTimer()
    {
        enemiesHit = new List<int>();
        Invoke ("DestroyRock", rockLifetime);
    }

    public void CancelDestructionTimer()
    {
        enemiesHit = new List<int>();
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
        enemiesHit = new List<int>();
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

    public void NewEnemyHit(int enemyId)
    {
        enemiesHit.Add(enemyId);
    }

    public bool EnemyWasHit(int enemyId)
    {
        return enemiesHit.Contains(enemyId);
    }

    public IEnumerator TempAddEnemy(int enemyId)
    {
        enemiesHit.Add(enemyId);
        yield return new WaitForSeconds(rockPunchTimeDelay);
        if(enemiesHit.Contains(enemyId))
        {
            enemiesHit.Remove(enemyId);
        }
    }

    public Rigidbody GetRigidbody()
    {
        return rockRigidbody;
    }

    public SkinnedMeshRenderer GetMeshRenderer()
    {
        return rockMesh;
    }

    public AudioSource GetActiveRockAudioSource()
    {
        return activeRockAudioSource;
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