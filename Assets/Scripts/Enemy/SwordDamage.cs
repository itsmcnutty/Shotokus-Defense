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
        parentEnemy = GetTopParent(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject otherTopParent = GetTopParent(other.gameObject);
        
        // Deal damage if colliding with one of player's colliders and enemy is in "Attacking" animation
        if (otherTopParent.Equals(player) &&
            parentEnemy.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsTag("Swinging"))
        {
            player.GetComponentInChildren<PlayerHealth>()?.TakeDamage(DAMAGE);
        }
    }

    // Get parent object at top of hierarchy by stepping trough parents until none exist
    private GameObject GetTopParent(GameObject obj)
    {
        Transform parentTransform = obj.transform;
        
        while (parentTransform.parent)
        {
            parentTransform = parentTransform.parent.transform;
        }
        
        return parentTransform.gameObject;
    }
}
