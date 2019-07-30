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
		public LayerMask floorFixupTraceLayerMask;
		public float floorFixupMaximumTraceDistance = 1.0f;
		public Material areaVisibleMaterial;
		public Material areaLockedMaterial;
		public Material areaHighlightedMaterial;
		public Material pointVisibleMaterial;
		public Material pointLockedMaterial;
		public Material pointHighlightedMaterial;
		public Transform destinationReticleTransform;
		public Transform invalidReticleTransform;
		public Color pointerValidColor;
		public Color pointerInvalidColor;
		public Color pointerLockedColor;
		public bool showPlayAreaMarker = true;

		public float teleportFadeTime = 0.1f;

		public float arcDistance = 10.0f;

		[Header ("Effects")]
		public Transform onActivateObjectTransform;
		public Transform onDeactivateObjectTransform;
		public float activateObjectTime = 1.0f;
		public float deactivateObjectTime = 1.0f;

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

		private float meshAlphaPercent = 1.0f;
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

		private Transform playAreaPreviewTransform;

		private bool originalHoverLockState = false;
		private Interactable originalHoveringInteractable = null;
		private AllowTeleportWhileAttachedToHand allowTeleportWhileAttached = null;

		private Vector3 startingFeetOffset = Vector3.zero;
		private bool movedFeetFarEnough = false;

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

			//If something is attached to the hand that is preventing teleport
			if (allowTeleportWhileAttached && !allowTeleportWhileAttached.teleportAllowed)
			{
				HidePointer ();
			}
			else
			{
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
			}

			if (visible)
			{
				UpdatePointer ();

				if (onActivateObjectTransform.gameObject.activeSelf && Time.time - pointerShowStartTime > activateObjectTime)
				{
					onActivateObjectTransform.gameObject.SetActive (false);
				}
			}
			else
			{
				if (onDeactivateObjectTransform.gameObject.activeSelf && Time.time - pointerHideStartTime > deactivateObjectTime)
				{
					onDeactivateObjectTransform.gameObject.SetActive (false);
				}
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

				if (showPlayAreaMarker)
				{
					//Show the play area marker if this is a teleport area
					// TeleportArea teleportArea = pointedAtTeleportMarker as TeleportArea;
					// if ( teleportArea != null && !teleportArea.locked && playAreaPreviewTransform != null )
					// {
					// 	Vector3 offsetToUse = playerFeetOffset;

					// 	//Adjust the actual offset to prevent the play area marker from moving too much
					// 	if ( !movedFeetFarEnough )
					// 	{
					// 		float distanceFromStartingOffset = Vector3.Distance( playerFeetOffset, startingFeetOffset );
					// 		if ( distanceFromStartingOffset < 0.1f )
					// 		{
					// 			offsetToUse = startingFeetOffset;
					// 		}
					// 		else if ( distanceFromStartingOffset < 0.4f )
					// 		{
					// 			offsetToUse = Vector3.Lerp( startingFeetOffset, playerFeetOffset, ( distanceFromStartingOffset - 0.1f ) / 0.3f );
					// 		}
					// 		else
					// 		{
					// 			movedFeetFarEnough = true;
					// 		}
					// 	}

					// 	playAreaPreviewTransform.position = pointedAtPosition + offsetToUse;

					// 	showPlayAreaPreview = true;
					// }
				}

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

			if (playAreaPreviewTransform != null)
			{
				playAreaPreviewTransform.gameObject.SetActive (showPlayAreaPreview);
			}

			destinationReticleTransform.position = pointedAtPosition;
			invalidReticleTransform.position = pointerEnd;
			onActivateObjectTransform.position = pointerEnd;
			onDeactivateObjectTransform.position = pointerEnd;

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
			if (pointerHand)
			{
				if (ShouldOverrideHoverLock ())
				{
					//Restore the original hovering interactable on the hand
					if (originalHoverLockState == true)
					{
						pointerHand.HoverLock (originalHoveringInteractable);
					}
					else
					{
						pointerHand.HoverUnlock (null);
					}
				}
			}
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

			if (playAreaPreviewTransform != null)
			{
				playAreaPreviewTransform.gameObject.SetActive (false);
			}

			if (onActivateObjectTransform.gameObject.activeSelf)
			{
				onActivateObjectTransform.gameObject.SetActive (false);
			}
			onDeactivateObjectTransform.gameObject.SetActive (true);

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

				startingFeetOffset = player.trackingOriginTransform.position - player.feetPositionGuess;
				movedFeetFarEnough = false;

				if (onDeactivateObjectTransform.gameObject.activeSelf)
				{
					onDeactivateObjectTransform.gameObject.SetActive (false);
				}
				onActivateObjectTransform.gameObject.SetActive (true);
			}

			if (oldPointerHand)
			{
				if (ShouldOverrideHoverLock ())
				{
					//Restore the original hovering interactable on the hand
					if (originalHoverLockState == true)
					{
						oldPointerHand.HoverLock (originalHoveringInteractable);
					}
					else
					{
						oldPointerHand.HoverUnlock (null);
					}
				}
			}

			pointerHand = newPointerHand;

			if (pointerHand)
			{
				pointerStartTransform = pointerHand.transform;

				if (pointerHand.currentAttachedObject != null)
				{
					allowTeleportWhileAttached = pointerHand.currentAttachedObject.GetComponent<AllowTeleportWhileAttachedToHand> ();
				}

				//Keep track of any existing hovering interactable on the hand
				originalHoverLockState = pointerHand.hoverLocked;
				originalHoveringInteractable = pointerHand.hoveringInteractable;

				if (ShouldOverrideHoverLock ())
				{
					pointerHand.HoverLock (null);
				}
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

			// TeleportPoint teleportPoint = teleportingToMarker as TeleportPoint;
			Vector3 teleportPosition = pointedAtPosition;

			// if ( teleportPoint != null )
			// {
			// 	teleportPosition = teleportPoint.transform.position;

			// 	//Teleport to a new scene
			// 	if ( teleportPoint.teleportType == TeleportPoint.TeleportPointType.SwitchToNewScene )
			// 	{
			// 		teleportPoint.TeleportToScene();
			// 		return;
			// 	}
			// }

			// Find the actual floor position below the navigation mesh
			// TeleportArea teleportArea = teleportingToMarker as TeleportArea;
			// if ( teleportArea != null )
			// {
			// 	if ( floorFixupMaximumTraceDistance > 0.0f )
			// 	{
			// 		RaycastHit raycastHit;
			// 		if ( Physics.Raycast( teleportPosition + 0.05f * Vector3.down, Vector3.down, out raycastHit, floorFixupMaximumTraceDistance, floorFixupTraceLayerMask ) )
			// 		{
			// 			teleportPosition = raycastHit.point;
			// 		}
			// 	}
			// }

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

				//Something is attached to the hand
				if (hand.currentAttachedObject != null)
				{
					AllowTeleportWhileAttachedToHand allowTeleportWhileAttachedToHand = hand.currentAttachedObject.GetComponent<AllowTeleportWhileAttachedToHand> ();

					if (allowTeleportWhileAttachedToHand != null && allowTeleportWhileAttachedToHand.teleportAllowed == true)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
			}

			return true;
		}

		//-------------------------------------------------
		private bool ShouldOverrideHoverLock ()
		{
			if (!allowTeleportWhileAttached || allowTeleportWhileAttached.overrideHoverLock)
			{
				return true;
			}

			return false;
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