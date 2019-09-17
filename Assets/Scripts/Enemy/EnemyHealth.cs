using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : CallParentCollision
{
	// Scalar value to compute damage from impulse
	private static float IMPULSE_MULTIPLIER = 0.034f;
	// Height at which to kill falling enemy
	private static float DEATH_Y = -10.0f;

	// The enemy's max health
	public float MAX_HEALTH;
	// How much damage a hit must deal to pierce armor reduction completely
	public float ARMOR_CUTOFF;
	// How effective armor is when hit by something below cutoff
	public float ARMOR_PROFICIENCY;
	// How much damage a hit must deal to send the enemy ragdolling
	public float RAGDOLL_DMG_THRESHOLD;
	
	// Show health value for debugging
	public bool debugShowHealthText = false;
	// UI canvas containing healthbar elements
	public Canvas healthBarCanvas;
	// Text that displays the enemy's health
	public Text healthBarText;
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
	// Enemy's ragdoll controller
	private RagdollController ragdollController;
	// Player camera
	private GameObject camera;

	// Start is called before the first frame update
	void Start()
	{
		// Instantiating stuff
		health = MAX_HEALTH;
		ragdollController = GetComponent<RagdollController>();
		camera = GameObject.FindGameObjectWithTag("MainCamera");
		
		// Setup health bars
		healthBarActual.minValue = 0f;
		healthBarActual.maxValue = MAX_HEALTH;
		healthBarActual.SetValueWithoutNotify(MAX_HEALTH);
		healthBarBefore.minValue = 0f;
		healthBarBefore.maxValue = MAX_HEALTH;
		healthBarBefore.SetValueWithoutNotify(MAX_HEALTH);
		UpdateHealthString();
	}

	// Called by child when receiving a collision event or a collision from its child (used
	// for receiving damage)
	protected override void OnCollisionEnterChild(GameObject child, Collision other)
	{
		// Do nothing if enemy hit was not hit with a weapon
		if (!IsRockWeapon(other.gameObject))
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

		// Calculate damage received
		float damage = CalculateDamage(other);
		
		// Update health and health bar
		health -= damage;
		healthBarActual.SetValueWithoutNotify(health);
		UpdateHealthString();
		
		// Get linear rate of decrease for ghost damage
		healthBarBeforeDecRate = (health - healthBeforeDamage) / REMOVE_DAMAGE_DURATION;

		// Begin ragdolling if taken enough damage and not already ragdolling
		if (damage >= RAGDOLL_DMG_THRESHOLD && !ragdollController.IsRagdolling())
		{
			ragdollController.StartRagdoll();
		}

		if (health <= 0f)
		{
			ragdollController.StartRagdoll();
			KillEnemy();
		}
	}

	// Removes enemy from game and checks for next round
	private void KillEnemy()
	{
		// Indicate the Game Controller that an enemy was destroyed
		GameController.Instance.EnemyGotDestroyed();
		// Check if round is over or not
		GameController.Instance.OnEnemyDeathClear();
	}

	private float CalculateDamage(Collision other)
	{
		float momentum  = other.gameObject.GetComponent<RockCollide>().GetMomentum();
		
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
	private bool IsRockWeapon(GameObject obj)
	{
		return obj.GetComponent<RockCollide>() != null;
	}
	
	// Updates the text above the health bar based on the enemy's current health
	private void UpdateHealthString()
	{
		if (debugShowHealthText)
		{
			healthBarText.text = String.Format("{0} / {1}", Math.Ceiling(health), Math.Ceiling(MAX_HEALTH));
		}
		else
		{
			healthBarText.text = "";
		}
	}

	// Checks for death plane and controls the animation of the "before" health bar
	private void Update()
	{
		if (transform.position.y <= DEATH_Y)
		{
			// Passed death plane
			KillEnemy();
			ragdollController.StopRagdoll();
		}
		
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
		
		// Rotate health bar to face player
		Vector3 cameraEulers = camera.transform.rotation.eulerAngles;
		healthBarCanvas.transform.rotation = Quaternion.Euler(cameraEulers.x, cameraEulers.y + 180, 0);
	}
}