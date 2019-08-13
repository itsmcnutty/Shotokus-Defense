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
	}
}