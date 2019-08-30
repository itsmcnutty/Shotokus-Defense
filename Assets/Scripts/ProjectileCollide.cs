using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCollide : MonoBehaviour
{
    public float PROJECTILE_DAMAGE = 100;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("PlayerCollider"))
        {
            Debug.Log("Damaging the player");
            other.gameObject.GetComponent<PlayerHealth>().TakeDamage(PROJECTILE_DAMAGE);
            // todo put this back into the code
//            Destroy(this.gameObject);
        }
    }

}
