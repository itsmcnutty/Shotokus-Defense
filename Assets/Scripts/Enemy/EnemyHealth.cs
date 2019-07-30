using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
	// Scalar value to compute damage from impulse
	private static float IMPULSE_MULTIPLIER = 0.4f;

	// The enemy's max health
	public float MAX_HEALTH;

	// How much damage a hit must deal to pierce armor reduction completely
	public float ARMOR_CUTOFF;

	// How effective armor is when hit by something below cutoff
	public float ARMOR_PROFICIENCY;

	private float health;

	// EVENTS STUFF DELETE IF other method works
//	public delegate void EnemyDeathEvent();
//	public event EnemyDeathEvent OnEnemyDeath;
	
	// public event Action<EnemyHealth> OnEnemyDeath; //  testing
	// Reference to game controller in order to update how many enemies have been destroyed
	public GameController gameController;


	// Start is called before the first frame update
	void Start()
	{
		health = MAX_HEALTH;
	}

	private void OnCollisionEnter(Collision other)
	{
		health -= calculateDamage(other.impulse.magnitude);
		

		if (health <= 0f)
		{
			Debug.Log("Enemy Died");
			// Testing subscriptions -------------------
//			if(OnEnemyDeath != null) {
//				Debug.Log("SUBSCRIBE TO MEEEEEEEEEEEEEEEEEEEE");
//				OnEnemyDeath();
//			}
			Destroy(gameObject);
			// Indicate the Game Controller that an enemy was destroyed
			gameController.enemyGotDestroyed();
			// ------------
		}
		
		Debug.Log("Collision: " + health);
	}

	private float calculateDamage(float impulse)
	{
		// Raw incoming damage
		float damage = IMPULSE_MULTIPLIER * impulse;

		// Determine whether armor will reduce damage
		if (damage < ARMOR_CUTOFF)
		{
			// Armor damage reduction
			// actual_dmg = (1 / (cutoff ^ (prof - 1))) * raw_dmg ^ prof
			return (1f / (float) Math.Pow(ARMOR_CUTOFF, ARMOR_PROFICIENCY - 1f)) *
			       (float) Math.Pow(damage, ARMOR_PROFICIENCY);
		}

		return damage;
	}
}