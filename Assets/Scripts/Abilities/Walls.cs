using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Walls : MonoBehaviour
{
    public GameObject wallPrefab;
    public float wallMaxHeight = 2f;
    public float wallSizeMultiplier = 120f;
    public float wallSpeedReduction = 50f;
    public float wallButtonClickDelay = 0.05f;

    private GameObject wallOutlinePrefab;
    private PlayerEnergy playerEnergy;
    private Material validOutlineMat;
    private Material invalidOutlineMat;
    private LayerMask outlineLayerMask;
    private GameObject player;
    private float rockCreationDistance;

    private static GameObject wallOutline;
    private static GameObject wall;
    private static Hand firstHandHeld;
    private static Hand firstHandReleased;
    private static float lastAngle;
    private static float startingHandHeight;
    private static float currentWallHeight;
    private Queue<Vector3> previousVelocities = new Queue<Vector3>();

    private NavMeshSurface surface;
    private NavMeshSurface surfaceLight;
    private NavMeshSurface surfaceWalls;

    public static Walls CreateComponent(GameObject player, GameObject wallOutlinePrefab, PlayerEnergy playerEnergy, Material validOutlineMat,
        Material invalidOutlineMat, float rockCreationDistance, LayerMask outlineLayerMask)
    {
        Walls walls = player.GetComponent<Walls>();

        walls.wallOutlinePrefab = wallOutlinePrefab;
        walls.playerEnergy = playerEnergy;
        walls.validOutlineMat = validOutlineMat;
        walls.invalidOutlineMat = invalidOutlineMat;
        walls.rockCreationDistance = rockCreationDistance;
        walls.outlineLayerMask = outlineLayerMask;
        walls.player = player;
        walls.surfaceWalls = GameObject.FindGameObjectWithTag("NavMesh Walls").GetComponent<NavMeshSurface>();

        return walls;
    }

    public void CreateNewWall(Hand hand, Hand otherHand)
    {
        if (firstHandHeld != null && firstHandHeld != hand)
        {
            OutlineProperties properties = wallOutline.GetComponentInChildren<OutlineProperties>();
            if (WallIsValid(hand.GetComponentInChildren<ControllerArc>(), otherHand.GetComponentInChildren<ControllerArc>()))
            {
                wall = Instantiate(wallPrefab) as GameObject;
                wall.transform.position = new Vector3(wallOutline.transform.position.x, wallOutline.transform.position.y + wallMaxHeight, wallOutline.transform.position.z);
                wall.transform.localScale = wallOutline.transform.localScale;
                wall.transform.rotation = wallOutline.transform.rotation;
                startingHandHeight = Math.Min(hand.transform.position.y, otherHand.transform.position.y);
                playerEnergy.SetTempEnergy(firstHandHeld, 0);

                ParticleSystem particleSystem = wall.GetComponentInChildren<ParticleSystem>();
                UnityEngine.ParticleSystem.ShapeModule shape = particleSystem.shape;
                shape.scale = new Vector3(shape.scale.x * wall.transform.localScale.x, shape.scale.y, shape.scale.z);
                wall.GetComponentInChildren<ParticleSystem>().Play();
                Destroy(wallOutline);
            }
            else
            {
                Destroy(wallOutline);
                ResetWallInfo();
            }
            firstHandReleased = null;
        }
        else
        {
            firstHandHeld = hand;
        }
    }

    public void UpdateWallHeight(Hand hand, Hand otherHand, SteamVR_Behaviour_Pose controllerPose)
    {
        hand.TriggerHapticPulse(1500);
        float newHandHeight = (Math.Min(hand.transform.position.y, otherHand.transform.position.y) - startingHandHeight) * wallMaxHeight;

        if (newHandHeight < 1 && currentWallHeight < newHandHeight)
        {
            float heightDifference = (wallMaxHeight * newHandHeight) - (wallMaxHeight * currentWallHeight);
            float newWallPosY = wall.transform.position.y + heightDifference;
            currentWallHeight = newHandHeight;

            Vector3 newPos = new Vector3(wall.transform.position.x, newWallPosY, wall.transform.position.z);
            wall.transform.position = Vector3.MoveTowards(wall.transform.position, newPos, 1f);

            ParticleSystem particleSystem = wall.GetComponentInChildren<ParticleSystem>();
            Vector3 particleSystemPos = particleSystem.transform.position;
            particleSystemPos = new Vector3(particleSystemPos.x, wall.transform.position.y - (wallMaxHeight * newHandHeight), particleSystemPos.z);
            particleSystem.transform.position = particleSystemPos;
            
            MeshRenderer meshRenderer = wallPrefab.GetComponentInChildren<MeshRenderer>();
            float area = (float) Math.Round(wall.transform.localScale.x * meshRenderer.bounds.size.x * wallMaxHeight * newHandHeight, 2) * wallSizeMultiplier;
            playerEnergy.SetTempEnergy(firstHandHeld, area);
        }

        previousVelocities.Enqueue(new Vector3(controllerPose.GetVelocity().x, 0, controllerPose.GetVelocity().z));
        if (previousVelocities.Count > 5)
        {
            previousVelocities.Dequeue();
        }
    }

    public void EndCreateWall(Hand hand, Hand otherHand, SteamVR_Behaviour_Pose controllerPose)
    {
        float finalHandHeight = (Math.Min(hand.transform.position.y, otherHand.transform.position.y) - startingHandHeight) * wallMaxHeight;
        UnityEngine.ParticleSystem.MainModule main = wall.GetComponentInChildren<ParticleSystem>().main;
        main.loop = false;
        if (finalHandHeight < 0.18f)
        {
            Destroy(wall);
            playerEnergy.CancelEnergyUsage(firstHandHeld);
        }
        else
        {
            wall.AddComponent<WallProperties>();
            wall.GetComponent<WallProperties>().wallHeightPercent = finalHandHeight;

            playerEnergy.UseEnergy(firstHandHeld);
            if (PlayerAbility.WallPushEnabled())
            {
                Vector3 finalVelocity = Vector3.zero;
                foreach (Vector3 velocity in previousVelocities)
                {
                    finalVelocity += velocity;
                }
                finalVelocity /= previousVelocities.Count;

                wall.GetComponent<WallProperties>().direction = finalVelocity.normalized;
                wall.GetComponent<WallProperties>().wallMoveSpeed = finalVelocity.magnitude / wallSpeedReduction;
            }
            else
            {
                PowerupController.IncrementWallPushCounter();
            }
            wall.GetComponent<CreateNavLink>().createLinks(wallMaxHeight);
            surfaceWalls.BuildNavMesh();
        }
        ResetWallInfo();
    }

    public void CancelWall(Hand hand, Hand otherHand)
    {
        UnityEngine.ParticleSystem.MainModule main = wall.GetComponentInChildren<ParticleSystem>().main;
        main.loop = false;
        wall.AddComponent<WallProperties>();
        wall.GetComponent<WallProperties>().wallHeightPercent = (Math.Min(hand.transform.position.y, otherHand.transform.position.y) - startingHandHeight) * wallMaxHeight;
        playerEnergy.UseEnergy(firstHandHeld);
        ResetWallInfo();
    }

    public void CancelWallOutline()
    {
        Destroy(wallOutline);
        ResetWallInfo();
    }

    private float CalculateOutlineVerticleCorrection(GameObject outline, out bool outOfBounds)
    {
        float verticleCorrection = 0;
        RaycastHit hit;
        if (Physics.Raycast(outline.transform.position + Vector3.up, Vector3.down, out hit, 1f, outlineLayerMask) ||
            Physics.Raycast(outline.transform.position, Vector3.down, out hit, 1f, outlineLayerMask))
        {
            if (hit.collider.tag == "Ground")
            {
                verticleCorrection = hit.point.y - outline.transform.position.y;
            }
            outOfBounds = false;
        }
        else
        {
            outOfBounds = true;
        }
        return verticleCorrection;
    }

    private void WallButtonsNotSimultaneous()
    {
        firstHandHeld = null;
        firstHandReleased = null;
    }

    public void EnterDrawMode(Hand hand, Hand otherHand)
    {
        if (firstHandHeld != null && firstHandHeld != hand)
        {
            CancelInvoke("WallButtonsNotSimultaneous");
            wallOutline = Instantiate(wallOutlinePrefab) as GameObject;
            SetWallLocation(hand.GetComponentInChildren<ControllerArc>(), otherHand.GetComponentInChildren<ControllerArc>());
            firstHandHeld = null;
        }
        else
        {
            firstHandHeld = hand;
            Invoke("WallButtonsNotSimultaneous", wallButtonClickDelay);
        }
    }

    public void ActiveDrawMode(ControllerArc arc, ControllerArc otherArc)
    {
        playerEnergy.UpdateAbilityUseTime();
        SetWallLocation(arc, otherArc);
        SetOutlineMaterial(wallOutline, WallIsValid(arc, otherArc));
    }

    public void ExitDrawMode(Hand hand)
    {
        if (firstHandReleased != null && firstHandReleased != hand)
        {
            CancelInvoke("WallButtonsNotSimultaneous");
            Destroy(wallOutline);
            ResetWallInfo();
            firstHandReleased = null;
        }
        else
        {
            firstHandReleased = hand;
            Invoke("WallButtonsNotSimultaneous", wallButtonClickDelay);
        }
    }

    private void ResetWallInfo()
    {
        firstHandHeld = null;
        wallOutline = null;
        wall = null;
        lastAngle = 0;
        currentWallHeight = 0;
    }

    private void SetWallPosition(ControllerArc arc, ControllerArc otherArc)
    {
        Vector3 thisArcPos = arc.GetEndPosition();
        Vector3 otherArcPos = otherArc.GetEndPosition();

        float wallPosX = (thisArcPos.x + otherArcPos.x) / 2;
        float wallPosY = Math.Min(thisArcPos.y, otherArcPos.y);
        float wallPosZ = (thisArcPos.z + otherArcPos.z) / 2;
        wallOutline.transform.position = new Vector3(wallPosX, wallPosY, wallPosZ);

        MeshRenderer meshRenderer = wallPrefab.GetComponentInChildren<MeshRenderer>();
        float verticleCorrection = CalculateOutlineVerticleCorrection(wallOutline, out bool outOfBounds);
        verticleCorrection += wallMaxHeight - meshRenderer.bounds.size.y;
        wallOutline.transform.position += new Vector3(0, verticleCorrection, 0);
    }

    private void SetWallLocation(ControllerArc arc, ControllerArc otherArc)
    {
        SetWallPosition(arc, otherArc);

        MeshRenderer meshRenderer = wallPrefab.GetComponentInChildren<MeshRenderer>();

        float remainingEnergy = playerEnergy.GetRemainingEnergy();
        float maxHandDist = remainingEnergy / (wallSizeMultiplier * wallMaxHeight);
        float handDistance = (arc.GetEndPointsDistance(otherArc) < maxHandDist) ?
            arc.GetEndPointsDistance(otherArc) :
            maxHandDist;
        float wallWidth = ((handDistance - meshRenderer.bounds.size.x) / meshRenderer.bounds.size.x) + 1;;
        wallOutline.transform.localScale = new Vector3(wallWidth, wallOutline.transform.localScale.y, wallOutline.transform.localScale.z);

        float angle = Vector3.SignedAngle(arc.GetEndPosition() - otherArc.GetEndPosition(), wallOutline.transform.position, new Vector3(0, -1, 0));
        angle += Vector3.SignedAngle(wallOutline.transform.position, new Vector3(1, 0, 0), new Vector3(0, -1, 0));
        float newAngle = angle;
        angle -= lastAngle;
        if (Math.Abs(angle) >= 0.5f)
        {
            lastAngle = newAngle;
            wallOutline.transform.Rotate(0, angle, 0, Space.Self);
        }
    }

    private bool WallIsValid(ControllerArc arc, ControllerArc otherArc)
    {
        OutlineProperties properties = wallOutline.GetComponentInChildren<OutlineProperties>();
        return (arc.CanUseAbility() &&
            otherArc.CanUseAbility() &&
            !properties.CollisionDetected() &&
            Vector3.Distance(player.transform.position, wallOutline.transform.position) >= rockCreationDistance);
    }

    public bool WallIsActive()
    {
        return wall != null;
    }

    public bool WallOutlineIsActive()
    {
        return wallOutline != null;
    }

    private void SetOutlineMaterial(GameObject outlineObject, bool valid)
    {
        if (valid)
        {
            outlineObject.GetComponentInChildren<MeshRenderer>().material = validOutlineMat;
        }
        else
        {
            outlineObject.GetComponentInChildren<MeshRenderer>().material = invalidOutlineMat;
        }
    }
}