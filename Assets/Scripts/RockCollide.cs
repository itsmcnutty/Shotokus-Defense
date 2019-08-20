using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockCollide : MonoBehaviour
{
	// True if this object has crashed into an enemy
	private bool crashed = false;
	// Position before fixed update
	private Vector3 beforePos;
	// Position after fixed update
	private Vector3 afterPos;
	// Velocity as of last fixed update
	private Vector3 velocity = Vector3.zero;
	// Rigidbody attached to this object
	private Rigidbody rigidbody;
	
	void Start()
	{
		beforePos = transform.position;
		rigidbody = GetComponent<Rigidbody>();
	}

	void FixedUpdate()
	{
		// Calculate velocity in last physics update
		afterPos = transform.position;
		Vector3 deltaPos =  afterPos - beforePos;
		velocity = deltaPos / Time.fixedDeltaTime;
		
		// Update beforePos
		beforePos = afterPos;
	}

	private void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.CompareTag("Enemy"))
		{
			crashed = true;
		}
	}

	public float GetMomentum()
	{
		return rigidbody.mass * velocity.magnitude;
	}
}