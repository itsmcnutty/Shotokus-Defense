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

		private TeleportMarkerBase[] teleportMarkers;
		private TeleportMarkerBase pointedAtTeleportMarker;
		private TeleportMarkerBase teleportingToMarker;
		private Vector3 pointedAtPosition;
		private Vector3 prevPointedAtPosition;
		private bool teleporting = false;
		private float currentFadeTime = 0.0f;

		private float pointerShowStartTime = 0.0f;
		private float pointerHideStartTime = 0.0f;
		private bool meshFading = false;
		private float fullTintAlpha;

		private float invalidReticleMinScale = 0.2f;
		private float invalidReticleMaxScale = 1.0f;
		private float invalidReticleMinScaleDistance = 0.4f;
		private float invalidReticleMaxScaleDistance = 2.0f;
		private Vector3 invalidReticleScale = Vector3.one;
		private Quaternion invalidReticleTargetRotation = Quaternion.identity;

		private bool originalHoverLockState = false;
		private Interactable originalHoveringInteractable = null;

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

			int tintColorID = Shader.PropertyToID ("_TintColor");
			fullTintAlpha = pointVisibleMaterial.GetColor (tintColorID).a;

			teleportArc = GetComponent<TeleportArc> ();
			teleportArc.traceLayerMask = traceLayerMask;

			float invalidReticleStartingScale = invalidReticleTransform.localScale.x;
			invalidReticleMinScale *= invalidReticleStartingScale;
			invalidReticleMaxScale *= invalidReticleStartingScale;
		}

		//-------------------------------------------------
		void Start ()
		{
			teleportMarkers = GameObject.FindObjectsOfType<TeleportMarkerBase> ();

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
							TryTeleportPlayer ();
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

			HighlightSelected (hitTeleportMarker);

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

				pointedAtTeleportMarker = hitTeleportMarker;
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

				pointedAtTeleportMarker = null;

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

			foreach (TeleportMarkerBase teleportMarker in teleportMarkers)
			{
				if (teleportMarker != null && teleportMarker.markerActive && teleportMarker.gameObject != null)
				{
					teleportMarker.gameObject.SetActive (false);
				}
			}

			destinationReticleTransform.gameObject.SetActive (false);
			invalidReticleTransform.gameObject.SetActive (false);

			pointerHand = null;
		}

		//-------------------------------------------------
		private void ShowPointer (Hand newPointerHand, Hand oldPointerHand)
		{
			if (!visible)
			{
				pointedAtTeleportMarker = null;
				pointerShowStartTime = Time.time;
				visible = true;
				meshFading = true;

				teleportPointerObject.SetActive (false);
				teleportArc.Show ();

				foreach (TeleportMarkerBase teleportMarker in teleportMarkers)
				{
					if (teleportMarker.markerActive && teleportMarker.ShouldActivate (player.feetPositionGuess))
					{
						teleportMarker.gameObject.SetActive (true);
						teleportMarker.Highlight (false);
					}
				}
			}

			pointerHand = newPointerHand;

			if (pointerHand)
			{
				pointerStartTransform = pointerHand.transform;

				//Keep track of any existing hovering interactable on the hand
				originalHoverLockState = pointerHand.hoverLocked;
				originalHoveringInteractable = pointerHand.hoveringInteractable;
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
		private void TryTeleportPlayer ()
		{
			if (visible && !teleporting)
			{
				if (pointedAtTeleportMarker != null && pointedAtTeleportMarker.locked == false)
				{
					//Pointing at an unlocked teleport marker
					teleportingToMarker = pointedAtTeleportMarker;
				}
			}
		}

		//-------------------------------------------------
		private void TeleportPlayer ()
		{
			teleporting = false;

			Teleport.PlayerPre.Send (pointedAtTeleportMarker);

			SteamVR_Fade.Start (Color.clear, currentFadeTime);

			Vector3 teleportPosition = pointedAtPosition;

			if (teleportingToMarker.ShouldMovePlayer ())
			{
				Vector3 playerFeetOffset = player.trackingOriginTransform.position - player.feetPositionGuess;
				player.trackingOriginTransform.position = teleportPosition + playerFeetOffset;

				if (player.leftHand.currentAttachedObjectInfo.HasValue)
					player.leftHand.ResetAttachedTransform (player.leftHand.currentAttachedObjectInfo.Value);
				if (player.rightHand.currentAttachedObjectInfo.HasValue)
					player.rightHand.ResetAttachedTransform (player.rightHand.currentAttachedObjectInfo.Value);
			}
			else
			{
				teleportingToMarker.TeleportPlayer (pointedAtPosition);
			}

			Teleport.Player.Send (pointedAtTeleportMarker);
		}

		//-------------------------------------------------
		private void HighlightSelected (TeleportMarkerBase hitTeleportMarker)
		{
			if (pointedAtTeleportMarker != hitTeleportMarker) //Pointing at a new teleport marker
			{
				if (pointedAtTeleportMarker != null)
				{
					pointedAtTeleportMarker.Highlight (false);
				}

				if (hitTeleportMarker != null)
				{
					hitTeleportMarker.Highlight (true);

					prevPointedAtPosition = pointedAtPosition;
					PlayPointerHaptic (!hitTeleportMarker.locked);
				}
			}
			else if (hitTeleportMarker != null) //Pointing at the same teleport marker
			{
				if (Vector3.Distance (prevPointedAtPosition, pointedAtPosition) > 1.0f)
				{
					prevPointedAtPosition = pointedAtPosition;
					PlayPointerHaptic (!hitTeleportMarker.locked);
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