using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class MeleeDamage : MonoBehaviour
{
    
    // Damage dealt by melee swing
    public float DAMAGE;

    [Header("Audio")]
    public AudioSource hitSound;
    
    // Player camera (HMD)
    private GameObject player;
    // Parent enemy object
    private GameObject parentEnemy;
    private Animator parentAnimator;
    private bool hitPlayer;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        parentEnemy = GetTopParent(gameObject);
        parentAnimator = parentEnemy.GetComponent<Animator>();
    }

    private void Update() {
        if(!parentAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Swinging") && hitPlayer)
        {
            hitPlayer = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject otherTopParent = GetTopParent(other.gameObject);
        
        // Deal damage if colliding with one of player's colliders and enemy is in "Attacking" animation
        if (!hitPlayer && otherTopParent.Equals(player) && parentAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Swinging"))
        {
            hitPlayer = true;
            player.GetComponentInChildren<PlayerHealth>()?.TakeDamage(DAMAGE);
            
            // Play sound because successfully hitting player
            hitSound.Play();
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
