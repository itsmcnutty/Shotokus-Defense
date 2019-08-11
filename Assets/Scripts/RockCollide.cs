using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockCollide : MonoBehaviour
{

	private bool crashed = false;

	private void OnCollisionEnter(Collision other)
	{
		crashed = true;
		GetComponent<Rigidbody>().velocity = Vector3.up;
		GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		transform.position = new Vector3(-12, 2, -8.3f);
	}
}