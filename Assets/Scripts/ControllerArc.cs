﻿using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ControllerArc : MonoBehaviour
{
	public SteamVR_Action_Boolean grabAction;
	public SteamVR_Action_Boolean gripAction;

	public LayerMask traceLayerMask;
	public Transform destinationReticleTransform;
	public Transform invalidReticleTransform;
	public Color pointerValidColor;
	public Color pointerInvalidColor;
	public Color pointerLockedColor;

	public float arcDistance = 10.0f;

	public Hand hand;

	private LineRenderer pointerLineRenderer;
	private bool applyPoint = true;
	private Player player = null;
	private TeleportArc arc = null;

	private bool visible = false;

	private Vector3 pointedAtPosition;

	private float invalidReticleMinScale = 0.2f;
	private float invalidReticleMaxScale = 1.0f;
	private float invalidReticleMinScaleDistance = 0.4f;
	private float invalidReticleMaxScaleDistance = 2.0f;
	private Vector3 invalidReticleScale = Vector3.one;
	private Quaternion invalidReticleTargetRotation = Quaternion.identity;

	private bool canUseAbility;

	private static ControllerArc _instance;
	public static ControllerArc instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GameObject.FindObjectOfType<ControllerArc> ();
			}

			return _instance;
		}
	}

	void Awake ()
	{
		_instance = this;

		pointerLineRenderer = GetComponentInChildren<LineRenderer> ();

		arc = GetComponent<TeleportArc> ();
		arc.traceLayerMask = traceLayerMask;

		float invalidReticleStartingScale = invalidReticleTransform.localScale.x;
		invalidReticleMinScale *= invalidReticleStartingScale;
		invalidReticleMaxScale *= invalidReticleStartingScale;
	}

	void Start ()
	{
		HidePointer ();

		player = Valve.VR.InteractionSystem.Player.instance;

		if (player == null)
		{
			Destroy (this.gameObject);
		}
	}

	void Update ()
	{
		if (visible)
		{
			if (WasButtonReleased ()) { }
		}

		if (IsButtonDown ())
		{
			applyPoint = false;
		}
		else
		{
			applyPoint = true;
		}

		if (!visible && applyPoint)
		{
			ShowPointer ();
		}
		else if (visible)
		{
			if (!applyPoint && IsButtonDown ())
			{
				HidePointer ();
			}
			else
			{
				UpdatePointer ();
			}
		}
	}

	private void UpdatePointer ()
	{
		Vector3 pointerStart = hand.transform.position;
		Vector3 pointerEnd;
		Vector3 pointerDir = hand.transform.forward;
		bool hitSomething = false;

		Vector3 arcVelocity = pointerDir * arcDistance;

		AbilityUsageMarker hitMarker = null;

		float dotUp = Vector3.Dot (pointerDir, Vector3.up);
		float dotForward = Vector3.Dot (pointerDir, player.hmdTransform.forward);
		bool pointerAtBadAngle = false;
		if ((dotForward > 0 && dotUp > 0.75f) || (dotForward < 0.0f && dotUp > 0.5f))
		{
			pointerAtBadAngle = true;
		}

		RaycastHit hitInfo;
		arc.SetArcData (pointerStart, arcVelocity, true, pointerAtBadAngle);
		if (arc.DrawArc (out hitInfo))
		{
			hitSomething = true;
			hitMarker = hitInfo.collider.GetComponentInParent<AbilityUsageMarker> ();
		}

		if (pointerAtBadAngle)
		{
			hitMarker = null;
		}

		if (hitMarker != null)
		{
			if (hitMarker.locked)
			{
				arc.SetColor (pointerLockedColor);
				pointerLineRenderer.startColor = pointerLockedColor;
				pointerLineRenderer.endColor = pointerLockedColor;
				destinationReticleTransform.gameObject.SetActive (false);

				canUseAbility = false;
			}
			else
			{
				arc.SetColor (pointerValidColor);
				pointerLineRenderer.startColor = pointerValidColor;
				pointerLineRenderer.endColor = pointerValidColor;
				destinationReticleTransform.gameObject.SetActive (true);

				canUseAbility = true;
			}

			invalidReticleTransform.gameObject.SetActive (false);

			pointedAtPosition = hitInfo.point;

			pointerEnd = hitInfo.point;
		}
		else
		{
			canUseAbility = false;

			destinationReticleTransform.gameObject.SetActive (false);

			arc.SetColor (pointerInvalidColor);
			pointerLineRenderer.startColor = pointerInvalidColor;
			pointerLineRenderer.endColor = pointerInvalidColor;
			invalidReticleTransform.gameObject.SetActive (!pointerAtBadAngle);

			Vector3 normalToUse = hitInfo.normal;
			float angle = Vector3.Angle (hitInfo.normal, Vector3.up);
			if (angle < 15.0f)
			{
				normalToUse = Vector3.up;
			}
			invalidReticleTargetRotation = Quaternion.FromToRotation (Vector3.up, normalToUse);
			invalidReticleTransform.rotation = Quaternion.Slerp (invalidReticleTransform.rotation, invalidReticleTargetRotation, 0.1f);

			float distanceFromPlayer = Vector3.Distance (hitInfo.point, player.hmdTransform.position);
			float invalidReticleCurrentScale = Util.RemapNumberClamped (distanceFromPlayer, invalidReticleMinScaleDistance, invalidReticleMaxScaleDistance, invalidReticleMinScale, invalidReticleMaxScale);
			invalidReticleScale.x = invalidReticleCurrentScale;
			invalidReticleScale.y = invalidReticleCurrentScale;
			invalidReticleScale.z = invalidReticleCurrentScale;
			invalidReticleTransform.transform.localScale = invalidReticleScale;

			if (hitSomething)
			{
				pointerEnd = hitInfo.point;
			}
			else
			{
				pointerEnd = arc.GetArcPositionAtTime (arc.arcDuration);
			}
		}

		destinationReticleTransform.position = pointedAtPosition;
		invalidReticleTransform.position = pointerEnd;

		pointerLineRenderer.SetPosition (0, pointerStart);
		pointerLineRenderer.SetPosition (1, pointerEnd);
	}

	private void HidePointer ()
	{
		visible = false;

		arc.Hide ();

		destinationReticleTransform.gameObject.SetActive (false);
		invalidReticleTransform.gameObject.SetActive (false);

		applyPoint = false;
	}

	private void ShowPointer ()
	{
		if (!visible)
		{
			visible = true;

			arc.Show ();
		}

		applyPoint = true;
	}

	public bool IsEligibleToUseAbility ()
	{
		if (hand == null)
		{
			return false;
		}

		if (!hand.gameObject.activeInHierarchy)
		{
			return false;
		}

		return true;
	}

	private bool WasButtonReleased ()
	{
		if (IsEligibleToUseAbility ())
		{
			return grabAction.GetStateUp (hand.handType) && gripAction.GetStateUp (hand.handType);
		}

		return false;
	}

	private bool IsButtonDown ()
	{
		if (IsEligibleToUseAbility ())
		{
			return grabAction.GetState (hand.handType) || gripAction.GetState (hand.handType);
		}

		return false;
	}

	private bool WasButtonPressed ()
	{
		if (IsEligibleToUseAbility ())
		{
			return grabAction.GetStateDown (hand.handType) || gripAction.GetStateDown (hand.handType);
		}

		return false;
	}

	public bool CanUseAbility() {
		return canUseAbility;
	}
}