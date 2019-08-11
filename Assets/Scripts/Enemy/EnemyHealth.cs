using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : CallParentCollision
{
	// Scalar value to compute damage from impulse
	private static float IMPULSE_MULTIPLIER = 0.4f;

	// The enemy's max health
	public float MAX_HEALTH;
	// How much damage a hit must deal to pierce armor reduction completely
	public float ARMOR_CUTOFF;
	// How effective armor is when hit by something below cutoff
	public float ARMOR_PROFICIENCY;
	
	// Slider which reflects actual health of enemy
	public Slider healthBarActual;
	// Slider which highlights how much damage the enemy took recently
	public Slider healthBarBefore;

	// How long to show the damage that was dealt in the "before" health bar (seconds)
	private float SHOW_DAMAGE_DURATION = 1f;
	// How long it takes to remove the damage that was dealt from the "before" health bar (seconds)
	private float REMOVE_DAMAGE_DURATION = 0.8f;
	
	// Seconds since last time enemy took damage
	private float timeSinceDamage;
	// HP before enemy took damage;
	private float healthBeforeDamage;
	// Rate at which ghost damage decreases
	private float healthBarBeforeDecRate;
	// Enemy's health points
	private float health;


	// Start is called before the first frame update
	void Start()
	{
		health = MAX_HEALTH;
		
		// Setup health bars
		healthBarActual.minValue = 0f;
		healthBarActual.maxValue = MAX_HEALTH;
		healthBarActual.SetValueWithoutNotify(MAX_HEALTH);
		healthBarBefore.minValue = 0f;
		healthBarBefore.maxValue = MAX_HEALTH;
		healthBarBefore.SetValueWithoutNotify(MAX_HEALTH);
	}

	// Called by child when receiving a collision event or a collision from its child (used
	// for receiving damage)
	protected override void OnCollisionEnterChild(GameObject child, Collision other)
	{
		// Do nothing if enemy hit was not hit with a weapon
		if (!IsWeapon(other.gameObject))
		{
			return;
		}
		
		// Set HP before damage to actual health if health bar is finished animating
		if (timeSinceDamage >= SHOW_DAMAGE_DURATION + REMOVE_DAMAGE_DURATION)
		{
			healthBeforeDamage = health;
		}
		// Otherwise set HP before damage to be what the ghost health currently shows
		else
		{
			healthBeforeDamage = healthBarBefore.value;
		}
		
		// Reset damage timer
		timeSinceDamage = 0f;
		
		// Get linear rate of decrease for ghost damage
		healthBarBeforeDecRate = (health - healthBeforeDamage) / REMOVE_DAMAGE_DURATION;

		// Update health and health bar
		health -= CalculateDamage(other);
		healthBarActual.SetValueWithoutNotify(health);

		// Begin ragdolling if not already ragdolling
		if (!GetComponent<RagdollController>().IsRagdolling())
		{
			GetComponent<RagdollController>().StartRagdoll();
		}

		if (health <= 0f)
		{
			Destroy(gameObject);
			// Indicate the Game Controller that an enemy was destroyed
			GameController.Instance.enemyGotDestroyed();
		}
	}

	private float CalculateDamage(Collision collision)
	{
		Rigidbody otherBody = collision.rigidbody;

		float momentum = (otherBody.mass * otherBody.velocity - collision.impulse).magnitude;
		
		// Raw incoming damage
		float damage = IMPULSE_MULTIPLIER * momentum;

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

	// Returns true if the specified GameObject can damage the enemy
	private bool IsWeapon(GameObject obj)
	{
		return obj.CompareTag("Rock") || obj.CompareTag("Spike");
	}

	// For controlling the animation of the "before" health bar
	private void Update()
	{
		timeSinceDamage += Time.deltaTime;

		// Animation complete
		if (timeSinceDamage >= REMOVE_DAMAGE_DURATION + SHOW_DAMAGE_DURATION)
		{
			healthBarBefore.SetValueWithoutNotify(health);
		}
		// Animate the decreasing ghost health
		else if (timeSinceDamage >= SHOW_DAMAGE_DURATION)
		{
			float decTime = timeSinceDamage - SHOW_DAMAGE_DURATION;
			healthBarBefore.SetValueWithoutNotify(healthBeforeDamage + healthBarBeforeDecRate * decTime);
		}
	}
}