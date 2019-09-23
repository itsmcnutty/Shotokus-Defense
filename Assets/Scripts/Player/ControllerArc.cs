using UnityEngine;
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

	private float invalidReticleMinScale = 0.2f;
	private float invalidReticleMaxScale = 1.0f;
	private float invalidReticleMinScaleDistance = 0.4f;
	private float invalidReticleMaxScaleDistance = 2.0f;
	private Vector3 reticleScale = Vector3.one;
	private Quaternion reticleTargetRotation = Quaternion.identity;

	private bool canUseAbility;
	private float distanceFromPlayer;
	private Vector3 pointerEnd;
	private GameObject pointerHitObject;

	private static ControllerArc _instance;
	public static ControllerArc instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GameObject.FindObjectOfType<ControllerArc>();
			}

			return _instance;
		}
	}

	void Awake()
	{
		_instance = this;

		pointerLineRenderer = GetComponentInChildren<LineRenderer>();

		arc = GetComponent<TeleportArc>();
		arc.traceLayerMask = traceLayerMask;

		float invalidReticleStartingScale = invalidReticleTransform.localScale.x;
		invalidReticleMinScale *= invalidReticleStartingScale;
		invalidReticleMaxScale *= invalidReticleStartingScale;
	}

	void Start()
	{
		HidePointer();

		player = Valve.VR.InteractionSystem.Player.instance;

		if (player == null)
		{
			Destroy(this.gameObject);
		}
	}

	void Update()
	{
		if (visible)
		{
			if (WasButtonReleased()) { }
		}

		if (IsButtonDown())
		{
			applyPoint = false;
		}
		else
		{
			applyPoint = true;
		}

		if (!visible && applyPoint)
		{
			ShowPointer();
		}
		else if (visible)
		{
			if (!applyPoint && IsButtonDown())
			{
				HidePointer();
			}
			else
			{
				UpdatePointer();
			}
		}
	}

	private void UpdatePointer()
	{
		Vector3 pointerStart = hand.transform.position;
		Vector3 pointerDir = hand.transform.forward;
		bool hitSomething = false;

		Vector3 arcVelocity = pointerDir * arcDistance;

		AbilityUsageMarker hitMarker = null;

		float dotUp = Vector3.Dot(pointerDir, Vector3.up);
		float dotForward = Vector3.Dot(pointerDir, player.hmdTransform.forward);
		bool pointerAtBadAngle = false;
		if ((dotForward > 0 && dotUp > 0.75f) || (dotForward < 0.0f && dotUp > 0.5f))
		{
			pointerAtBadAngle = true;
		}

		RaycastHit hitInfo;
		arc.SetArcData(pointerStart, arcVelocity, true, pointerAtBadAngle);
		if (arc.DrawArc(out hitInfo))
		{
			hitSomething = true;
			hitMarker = hitInfo.collider.GetComponentInParent<AbilityUsageMarker>();
			if (hitMarker == null)
			{
				hitMarker = hitInfo.collider.GetComponent<AbilityUsageMarker>();
			}
			Vector3 playerPosHitHeight = new Vector3(player.hmdTransform.position.x, hitInfo.point.y, player.hmdTransform.position.z);
			distanceFromPlayer = Vector3.Distance(hitInfo.point, playerPosHitHeight);
		}

		if (pointerAtBadAngle)
		{
			hitMarker = null;
		}

		bool validCollisionForAbility = false;

		if(hitMarker != null)
		{
			validCollisionForAbility = true;
			MeshRenderer meshRenderer = destinationReticleTransform.gameObject.GetComponent<MeshRenderer>();
			Debug.Log("size=" + meshRenderer.bounds.size);
			Collider[] colliders = Physics.OverlapSphere(hitInfo.point, meshRenderer.bounds.size.x / 2, traceLayerMask);
			Debug.Log("Found colliders=" + colliders.Length);
			foreach (Collider collider in colliders)
			{
				Debug.Log(collider.name);
				if (collider.tag != "Ground" && collider.gameObject.layer != 14)
				{
					validCollisionForAbility = false;
					break;
				}
			}
		}

		if (validCollisionForAbility)
		{
			if (hitMarker.locked)
			{
				arc.SetColor(pointerLockedColor);
				pointerLineRenderer.startColor = pointerLockedColor;
				pointerLineRenderer.endColor = pointerLockedColor;
				destinationReticleTransform.gameObject.SetActive(false);

				canUseAbility = false;
			}
			else
			{
				arc.SetColor(pointerValidColor);
				pointerLineRenderer.startColor = pointerValidColor;
				pointerLineRenderer.endColor = pointerValidColor;

				destinationReticleTransform.gameObject.SetActive(true);

				Vector3 normalToUse = hitInfo.normal;
				float angle = Vector3.Angle(hitInfo.normal, Vector3.up);
				if (angle < 15.0f)
				{
					normalToUse = Vector3.up;
				}
				reticleTargetRotation = Quaternion.FromToRotation(Vector3.up, normalToUse);
				destinationReticleTransform.rotation = Quaternion.Slerp(destinationReticleTransform.rotation, reticleTargetRotation, 0.1f);

				canUseAbility = true;
				if (hitInfo.collider.gameObject.GetComponent<Interactable>() != null)
				{
					hand.hoveringInteractable = hitInfo.collider.gameObject.GetComponent<Interactable>();
				}
			}

			invalidReticleTransform.gameObject.SetActive(false);

			pointerEnd = hitInfo.point;
			pointerHitObject = hitInfo.collider.gameObject;
		}
		else
		{
			canUseAbility = false;

			destinationReticleTransform.gameObject.SetActive(false);

			arc.SetColor(pointerInvalidColor);
			pointerLineRenderer.startColor = pointerInvalidColor;
			pointerLineRenderer.endColor = pointerInvalidColor;
			invalidReticleTransform.gameObject.SetActive(!pointerAtBadAngle);

			Vector3 normalToUse = hitInfo.normal;
			float angle = Vector3.Angle(hitInfo.normal, Vector3.up);
			if (angle < 15.0f)
			{
				normalToUse = Vector3.up;
			}
			reticleTargetRotation = Quaternion.FromToRotation(Vector3.up, normalToUse);
			invalidReticleTransform.rotation = Quaternion.Slerp(invalidReticleTransform.rotation, reticleTargetRotation, 0.1f);

			float invalidReticleCurrentScale = Util.RemapNumberClamped(distanceFromPlayer, invalidReticleMinScaleDistance, invalidReticleMaxScaleDistance, invalidReticleMinScale, invalidReticleMaxScale);
			reticleScale.x = invalidReticleCurrentScale;
			reticleScale.y = invalidReticleCurrentScale;
			reticleScale.z = invalidReticleCurrentScale;
			invalidReticleTransform.transform.localScale = reticleScale;

			if (hitSomething)
			{
				pointerEnd = hitInfo.point;
				pointerHitObject = hitInfo.collider.gameObject;
				if (hitInfo.collider.gameObject.GetComponent<Interactable>() != null)
				{
					hand.hoveringInteractable = hitInfo.collider.gameObject.GetComponent<Interactable>();
				}
			}
			else
			{
				pointerEnd = arc.GetArcPositionAtTime(arc.arcDuration);
				pointerHitObject = null;
			}
		}

		destinationReticleTransform.position = pointerEnd;
		invalidReticleTransform.position = pointerEnd;

		pointerLineRenderer.SetPosition(0, pointerStart);
		pointerLineRenderer.SetPosition(1, pointerEnd);
	}

	public void HidePointer()
	{
		visible = false;

		arc.Hide();

		destinationReticleTransform.gameObject.SetActive(false);
		invalidReticleTransform.gameObject.SetActive(false);

		applyPoint = false;
	}

	public void ShowPointer()
	{
		if (!visible)
		{
			visible = true;

			arc.Show();
		}

		applyPoint = true;
	}

	public bool IsEligibleToUseAbility()
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

	private bool WasButtonReleased()
	{
		if (IsEligibleToUseAbility())
		{
			return grabAction.GetStateUp(hand.handType) && gripAction.GetStateUp(hand.handType);
		}

		return false;
	}

	private bool IsButtonDown()
	{
		if (IsEligibleToUseAbility())
		{
			return grabAction.GetState(hand.handType) || gripAction.GetState(hand.handType);
		}

		return false;
	}

	private bool WasButtonPressed()
	{
		if (IsEligibleToUseAbility())
		{
			return grabAction.GetStateDown(hand.handType) || gripAction.GetStateDown(hand.handType);
		}

		return false;
	}

	public bool CanUseAbility()
	{
		return canUseAbility;
	}

	public void setCanUseAbility(bool isAllowed)
	{
		canUseAbility = isAllowed;
	}

	public float GetDistanceFromPlayer()
	{
		return distanceFromPlayer;
	}

	public Vector3 GetEndPosition()
	{
		return pointerEnd;
	}

	public float GetEndPointsDistance(ControllerArc otherArc)
	{
		return Vector3.Distance(otherArc.GetEndPosition(), pointerEnd);
	}

	public GameObject GetPointerHitObject()
	{
		return pointerHitObject;
	}

	public void ClearPointerHitObject()
	{
		pointerHitObject = null;
	}
}