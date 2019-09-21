using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockCollide : MonoBehaviour
{
	// Position before fixed update
	private Vector3 beforePos;
	// Position after fixed update
	private Vector3 afterPos;
	// Velocity as of last fixed update
	private Vector3 velocity = Vector3.zero;
	// Rigidbody attached to this object
	private Rigidbody rockRigidbody;
	
	void Start()
	{
		beforePos = transform.position;
		rockRigidbody = GetComponent<Rigidbody>();
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

	public float GetMomentum()
	{
		return rockRigidbody.mass * velocity.magnitude;
	}
}