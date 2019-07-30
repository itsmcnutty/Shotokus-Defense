//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Handles all the teleport logic
//
//=============================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class Test : MonoBehaviour
	{
		public SteamVR_Action_Boolean teleportAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean> ("Teleport");

		public LayerMask traceLayerMask;
		public Material pointVisibleMaterial;
		public Transform destinationReticleTransform;
		public Transform invalidReticleTransform;
		public Color pointerValidColor;
		public Color pointerInvalidColor;
		public Color pointerLockedColor;

		public float arcDistance = 10.0f;

		private LineRenderer pointerLineRenderer;
		private GameObject teleportPointerObject;
		private Transform pointerStartTransform;
		private Hand pointerHand = null;
		private Player player = null;
		private TeleportArc teleportArc = null;

		private bool visible = false;

		private Vector3 pointedAtPosition;

		private float pointerShowStartTime = 0.0f;
		private float pointerHideStartTime = 0.0f;
		
		private float invalidReticleMinScale = 0.2f;
		private float invalidReticleMaxScale = 1.0f;
		private float invalidReticleMinScaleDistance = 0.4f;
		private float invalidReticleMaxScaleDistance = 2.0f;
		private Vector3 invalidReticleScale = Vector3.one;
		private Quaternion invalidReticleTargetRotation = Quaternion.identity;

		//-------------------------------------------------
		private static Test _instance;
		public static Test instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = GameObject.FindObjectOfType<Test> ();
				}

				return _instance;
			}
		}

		//-------------------------------------------------
		void Awake ()
		{
			_instance = this;

			pointerLineRenderer = GetComponentInChildren<LineRenderer> ();
			teleportPointerObject = pointerLineRenderer.gameObject;

			teleportArc = GetComponent<TeleportArc> ();
			teleportArc.traceLayerMask = traceLayerMask;

			float invalidReticleStartingScale = invalidReticleTransform.localScale.x;
			invalidReticleMinScale *= invalidReticleStartingScale;
			invalidReticleMaxScale *= invalidReticleStartingScale;
		}

		//-------------------------------------------------
		void Start ()
		{
			HidePointer ();

			player = InteractionSystem.Player.instance;

			if (player == null)
			{
				Destroy (this.gameObject);
				return;
			}
		}

		//-------------------------------------------------
		public void HideTeleportPointer ()
		{
			if (pointerHand != null)
			{
				HidePointer ();
			}
		}

		//-------------------------------------------------
		void Update ()
		{
			Hand oldPointerHand = pointerHand;
			Hand newPointerHand = null;

			foreach (Hand hand in player.hands)
			{
				if (visible)
				{
					if (WasTeleportButtonReleased (hand))
					{
						if (pointerHand == hand) //This is the pointer hand
						{
						}
					}
				}

				if (WasTeleportButtonPressed (hand))
				{
					newPointerHand = hand;
				}
			}

			if (!visible && newPointerHand != null)
			{
				//Begin showing the pointer
				ShowPointer (newPointerHand, oldPointerHand);
			}
			else if (visible)
			{
				if (newPointerHand == null && !IsTeleportButtonDown (pointerHand))
				{
					//Hide the pointer
					HidePointer ();
				}
				else if (newPointerHand != null)
				{
					//Move the pointer to a new hand
					ShowPointer (newPointerHand, oldPointerHand);
				}
			}

			if (visible)
			{
				UpdatePointer ();
			}
		}

		//-------------------------------------------------
		private void UpdatePointer ()
		{
			Vector3 pointerStart = pointerStartTransform.position;
			Vector3 pointerEnd;
			Vector3 pointerDir = pointerStartTransform.forward;
			bool hitSomething = false;
			bool showPlayAreaPreview = false;
			Vector3 playerFeetOffset = player.trackingOriginTransform.position - player.feetPositionGuess;

			Vector3 arcVelocity = pointerDir * arcDistance;

			TeleportMarkerBase hitTeleportMarker = null;

			//Check pointer angle
			float dotUp = Vector3.Dot (pointerDir, Vector3.up);
			float dotForward = Vector3.Dot (pointerDir, player.hmdTransform.forward);
			bool pointerAtBadAngle = false;
			if ((dotForward > 0 && dotUp > 0.75f) || (dotForward < 0.0f && dotUp > 0.5f))
			{
				pointerAtBadAngle = true;
			}

			//Trace to see if the pointer hit anything
			RaycastHit hitInfo;
			teleportArc.SetArcData (pointerStart, arcVelocity, true, pointerAtBadAngle);
			if (teleportArc.DrawArc (out hitInfo))
			{
				hitSomething = true;
				hitTeleportMarker = hitInfo.collider.GetComponentInParent<TeleportMarkerBase> ();
			}

			if (pointerAtBadAngle)
			{
				hitTeleportMarker = null;
			}

			if (hitTeleportMarker != null) //Hit a teleport marker
			{
				if (hitTeleportMarker.locked)
				{
					teleportArc.SetColor (pointerLockedColor);
#if (UNITY_5_4)
					pointerLineRenderer.SetColors (pointerLockedColor, pointerLockedColor);
#else
					pointerLineRenderer.startColor = pointerLockedColor;
					pointerLineRenderer.endColor = pointerLockedColor;
#endif
					destinationReticleTransform.gameObject.SetActive (false);
				}
				else
				{
					teleportArc.SetColor (pointerValidColor);
#if (UNITY_5_4)
					pointerLineRenderer.SetColors (pointerValidColor, pointerValidColor);
#else
					pointerLineRenderer.startColor = pointerValidColor;
					pointerLineRenderer.endColor = pointerValidColor;
#endif
					destinationReticleTransform.gameObject.SetActive (hitTeleportMarker.showReticle);
				}

				invalidReticleTransform.gameObject.SetActive (false);

				pointedAtPosition = hitInfo.point;

				pointerEnd = hitInfo.point;
			}
			else //Hit neither
			{
				destinationReticleTransform.gameObject.SetActive (false);

				teleportArc.SetColor (pointerInvalidColor);
#if (UNITY_5_4)
				pointerLineRenderer.SetColors (pointerInvalidColor, pointerInvalidColor);
#else
				pointerLineRenderer.startColor = pointerInvalidColor;
				pointerLineRenderer.endColor = pointerInvalidColor;
#endif
				invalidReticleTransform.gameObject.SetActive (!pointerAtBadAngle);

				//Orient the invalid reticle to the normal of the trace hit point
				Vector3 normalToUse = hitInfo.normal;
				float angle = Vector3.Angle (hitInfo.normal, Vector3.up);
				if (angle < 15.0f)
				{
					normalToUse = Vector3.up;
				}
				invalidReticleTargetRotation = Quaternion.FromToRotation (Vector3.up, normalToUse);
				invalidReticleTransform.rotation = Quaternion.Slerp (invalidReticleTransform.rotation, invalidReticleTargetRotation, 0.1f);

				//Scale the invalid reticle based on the distance from the player
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
					pointerEnd = teleportArc.GetArcPositionAtTime (teleportArc.arcDuration);
				}
			}

			destinationReticleTransform.position = pointedAtPosition;
			invalidReticleTransform.position = pointerEnd;

			pointerLineRenderer.SetPosition (0, pointerStart);
			pointerLineRenderer.SetPosition (1, pointerEnd);
		}

		//-------------------------------------------------
		private void HidePointer ()
		{
			if (visible)
			{
				pointerHideStartTime = Time.time;
			}

			visible = false;
			teleportPointerObject.SetActive (false);

			teleportArc.Hide ();

			destinationReticleTransform.gameObject.SetActive (false);
			invalidReticleTransform.gameObject.SetActive (false);

			pointerHand = null;
		}

		//-------------------------------------------------
		private void ShowPointer (Hand newPointerHand, Hand oldPointerHand)
		{
			if (!visible)
			{
				pointerShowStartTime = Time.time;
				visible = true;

				teleportPointerObject.SetActive (false);
				teleportArc.Show ();
			}

			pointerHand = newPointerHand;

			if (pointerHand)
			{
				pointerStartTransform = pointerHand.transform;
			}
		}

		//-------------------------------------------------
		private void PlayPointerHaptic (bool validLocation)
		{
			if (pointerHand != null)
			{
				if (validLocation)
				{
					pointerHand.TriggerHapticPulse (800);
				}
				else
				{
					pointerHand.TriggerHapticPulse (100);
				}
			}
		}

		//-------------------------------------------------
		public bool IsEligibleForTeleport (Hand hand)
		{
			if (hand == null)
			{
				return false;
			}

			if (!hand.gameObject.activeInHierarchy)
			{
				return false;
			}

			if (hand.hoveringInteractable != null)
			{
				return false;
			}

			if (hand.noSteamVRFallbackCamera == null)
			{
				if (hand.isActive == false)
				{
					return false;
				}
			}

			return true;
		}

		//-------------------------------------------------
		private bool WasTeleportButtonReleased (Hand hand)
		{
			if (IsEligibleForTeleport (hand))
			{
				return teleportAction.GetStateUp (hand.handType);
			}

			return false;
		}

		//-------------------------------------------------
		private bool IsTeleportButtonDown (Hand hand)
		{
			if (IsEligibleForTeleport (hand))
			{
				return teleportAction.GetState (hand.handType);
			}

			return false;
		}

		//-------------------------------------------------
		private bool WasTeleportButtonPressed (Hand hand)
		{
			if (IsEligibleForTeleport (hand))
			{
				return teleportAction.GetStateDown (hand.handType);
			}

			return false;
		}
	}
}