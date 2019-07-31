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
        player = GameObject.FindGameObjectWithTag("MainCamera");
        parentEnemy = transform.parent.transform.parent.gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.gameObject.Equals(player));
        //Debug.Log(parentEnemy.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Slashing"));
        
        // Deal damage if colliding with player's body and enemy is in "Slashing" animation
        if (other.gameObject.Equals(player) &&
            parentEnemy.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Slashing"))
        {
            other.gameObject.GetComponent<PlayerHealth>().TakeDamage(DAMAGE);
            //Debug.Log("Hit!");
        }
        else
        {
            //Debug.Log("No hit!");
        }
    }
}
