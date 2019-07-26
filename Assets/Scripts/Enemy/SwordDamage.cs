using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class SwordDamage : MonoBehaviour
{
    
    // Damage dealt by sword attack
    public float DAMAGE;
    
    // Player camera (HMD)
    private GameObject player;
    // Parent enemy object
    private GameObject parentEnemy;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        parentEnemy = transform.parent.transform.parent.gameObject;
    }

    private void OnCollisionEnter(Collision other)
    {
        // Deal damage if colliding with player's body and enemy is in "Slashing" animation
        if (other.collider.Equals(player.GetComponent<CapsuleCollider>()) &&
            parentEnemy.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Slashing"))
        {
            other.gameObject.GetComponent<PlayerHealth>().TakeDamage(DAMAGE);
        }
    }
}
