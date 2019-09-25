using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Walls : MonoBehaviour
{
    public GameObject wallPrefab;
    public Material validWallOutlineMat;
    public float wallMaxHeight = 2f;
    public float wallSizeMultiplier = 120f;
    public float wallSpeedReduction = 50f;
    public float wallButtonClickDelay = 0.05f;
    public float wallMinHandMovement = .27f;

    [Header("Audio")]
    public AudioClip raiseWall;
    public AudioClip breakWall;
    public ParticleSystem wallCreateParticles;
    public ParticleSystem wallDestroyParticles;

    private GameObject wallOutlinePrefab;
    private PlayerEnergy playerEnergy;
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

    private static ParticleSystem currentParticles;

    private NavMeshSurface surface;
    private NavMeshSurface surfaceLight;
    private NavMeshSurface surfaceWalls;

    public static Walls CreateComponent(GameObject player, GameObject wallOutlinePrefab, PlayerEnergy playerEnergy, Material invalidOutlineMat,
        float rockCreationDistance, LayerMask outlineLayerMask)
    {
        Walls walls = player.GetComponent<Walls>();

        walls.wallOutlinePrefab = wallOutlinePrefab;
        walls.playerEnergy = playerEnergy;
        walls.invalidOutlineMat = invalidOutlineMat;
        walls.rockCreationDistance = rockCreationDistance;
        walls.outlineLayerMask = outlineLayerMask;
        walls.player = player;
        walls.surfaceWalls = GameObject.FindGameObjectWithTag("NavMesh Walls").GetComponent<NavMeshSurface>();

        return walls;
    }

    public void CreateNewWall(Hand hand, Hand otherHand)
    {
        // Checks that one trigger is being held and it's not the current hand
        if (firstHandHeld != null && firstHandHeld != hand)
        {
            // Checks that the wall is valid
            if (WallIsValid(hand.GetComponentInChildren<ControllerArc>(), otherHand.GetComponentInChildren<ControllerArc>()))
            {
                // Creates a wall with the given information of the outline, below the surface
                MeshRenderer meshRenderer = wallOutline.GetComponentInChildren<MeshRenderer>();
                float heightDifference = meshRenderer.bounds.size.y - wallMaxHeight;

                wall = Instantiate(wallPrefab) as GameObject;
                wall.transform.position = new Vector3(wallOutline.transform.position.x, wallOutline.transform.position.y + heightDifference, wallOutline.transform.position.z);
                wall.transform.localScale = wallOutline.transform.localScale;
                wall.transform.rotation = wallOutline.transform.rotation;
                startingHandHeight = Math.Min(hand.transform.position.y, otherHand.transform.position.y);
                playerEnergy.SetTempEnergy(firstHandHeld, 0);

                // Plays the wall creation particle effects
                currentParticles = Instantiate(wallCreateParticles);
                currentParticles.transform.position = wall.transform.position;

                // Matches the shape of the particle area to that of the wall
                UnityEngine.ParticleSystem.ShapeModule shape = currentParticles.shape;
                shape.scale = new Vector3(shape.scale.x * wall.transform.localScale.x, shape.scale.y, shape.scale.z);

                // Scales the number of particles based on the size of the wall
                UnityEngine.ParticleSystem.EmissionModule emissionModule = currentParticles.emission;
                emissionModule.rateOverTimeMultiplier = shape.scale.x * 75;

                WallProperties.CreateComponent(wall, 0, wallDestroyParticles, breakWall);

                Destroy(wallOutline);
                //raiseWall.Play();
            }
            else
            {
                // Destroys an invalid wall
                Destroy(wallOutline);
                ResetWallInfo();
            }
            firstHandReleased = null;
        }
        else
        {
            // Sets this hand to be the first trigger pressed
            firstHandHeld = hand;
        }
    }

    public void UpdateWallHeight(Hand hand, Hand otherHand, SteamVR_Behaviour_Pose controllerPose)
    {
        hand.TriggerHapticPulse(1500);
        // Calculates the new height of your hands
        float newHandHeight = (Math.Min(hand.transform.position.y, otherHand.transform.position.y) - startingHandHeight) * wallMaxHeight;

        // Checks that the hand height is above the previous height and below the max scale size (100%)
        if (newHandHeight < 1 && currentWallHeight < newHandHeight)
        {
            // Calulculates the increase in wall height
            float heightDifference = (wallMaxHeight * newHandHeight) - (wallMaxHeight * currentWallHeight);
            float newWallPosY = wall.transform.position.y + heightDifference;
            currentWallHeight = newHandHeight;

            // Sets the new position of the wall
            Vector3 newPos = new Vector3(wall.transform.position.x, newWallPosY, wall.transform.position.z);
            wall.transform.position = Vector3.MoveTowards(wall.transform.position, newPos, 1f);

            // Calculates the amount of energy used by the wall
            MeshRenderer meshRenderer = wallPrefab.GetComponentInChildren<MeshRenderer>();
            float area = (float) Math.Round(wall.transform.localScale.x * meshRenderer.bounds.size.x * wallMaxHeight * newHandHeight, 2) * wallSizeMultiplier;
            playerEnergy.SetTempEnergy(firstHandHeld, area);
        }

        // Saves the previous five velocities for the wall push ability
        previousVelocities.Enqueue(new Vector3(controllerPose.GetVelocity().x, 0, controllerPose.GetVelocity().z));
        if (previousVelocities.Count > 5)
        {
            previousVelocities.Dequeue();
        }
    }

    public void EndCreateWall(Hand hand, Hand otherHand, SteamVR_Behaviour_Pose controllerPose)
    {
        // Calculates the final hand height
        float finalHandHeight = (Math.Min(hand.transform.position.y, otherHand.transform.position.y) - startingHandHeight) * wallMaxHeight;

        // Ends the creation particle loop
        UnityEngine.ParticleSystem.MainModule main = currentParticles.main;
        main.loop = false;
        Vector3 finalVelocity = Vector3.zero;
        float wallMoveSpeed = 0;

        if (PlayerAbility.WallPushEnabled)
        {
            // Sets the velocity of the wall to the average of the velocities if wall push is enabled
            foreach (Vector3 velocity in previousVelocities)
            {
                finalVelocity += velocity;
            }
            finalVelocity /= previousVelocities.Count;

            finalVelocity = finalVelocity.normalized;
            wallMoveSpeed = finalVelocity.magnitude / wallSpeedReduction;
        }
        else
        {
            // Increments wall push power-up when not enabled
            float wallEnergy = playerEnergy.GetEnergyForHand(firstHandHeld);
            PowerupController.IncrementWallPushCounter(wallEnergy);
        }

        if (finalHandHeight < wallMinHandMovement)
        {
            Destroy(wall);
        }
        else if(wall)
        {
            // Initializes the wall with the WallProperties component and creates a NavLink for wall climbing
            WallProperties.UpdateComponent(wall, finalHandHeight, finalVelocity, wallMoveSpeed);
            playerEnergy.UseEnergy(firstHandHeld);
            wall.GetComponentInChildren<CreateNavLink>().createLinks(wallMaxHeight);
            surfaceWalls.BuildNavMesh();
        }
        //raiseWall.Stop();
        ResetWallInfo();
    }

    public void CancelWall(Hand hand, Hand otherHand)
    {
        // Ends the wall creation particles
        UnityEngine.ParticleSystem.MainModule main = currentParticles.main;
        main.loop = false;

        // Calculates the final hand height and initializes the WallProperties component pn the wall
        float finalHandHeight = (Math.Min(hand.transform.position.y, otherHand.transform.position.y) - startingHandHeight) * wallMaxHeight;
        WallProperties.UpdateComponent(wall, finalHandHeight, Vector3.zero, 0);
        playerEnergy.UseEnergy(firstHandHeld);
        ResetWallInfo();
    }

    public void CancelWallOutline()
    {
        // Destroys and resets the wall
        Destroy(wallOutline);
        ResetWallInfo();
    }

    private void WallButtonsNotSimultaneous()
    {
        // Resets the first trigger held/released operations when the user isn't quick enough
        firstHandHeld = null;
        firstHandReleased = null;
    }

    public void EnterDrawMode(Hand hand, Hand otherHand)
    {
        // Checks if both hands are pressing the trigger
        if (firstHandHeld != null && firstHandHeld != hand)
        {
            // Cancels the invocation to prevent resetting trigger information
            CancelInvoke("WallButtonsNotSimultaneous");

            // Creates a new wall outline and sets its location
            wallOutline = Instantiate(wallOutlinePrefab) as GameObject;
            SetWallLocation(hand.GetComponentInChildren<ControllerArc>(), otherHand.GetComponentInChildren<ControllerArc>());
            firstHandHeld = null;
        }
        else
        {
            // Sets the first hand to press the trigger and starts the timer to press the other trigger
            firstHandHeld = hand;
            Invoke("WallButtonsNotSimultaneous", wallButtonClickDelay);
        }
    }

    public void ActiveDrawMode(ControllerArc arc, ControllerArc otherArc)
    {
        // Updates the wall location and material
        SetWallLocation(arc, otherArc);
        PlayerAbility.SetOutlineMaterial(wallOutline, WallIsValid(arc, otherArc), validWallOutlineMat, invalidOutlineMat);
    }

    public void ExitDrawMode(Hand hand)
    {
        // Checks if both hands are pressing the trigger
        if (firstHandReleased != null && firstHandReleased != hand)
        {
            // Cancels the invocation to prevent resetting trigger information
            CancelInvoke("WallButtonsNotSimultaneous");
            Destroy(wallOutline);
            ResetWallInfo();
            firstHandReleased = null;
        }
        else
        {
            // Sets the first hand to press the trigger and starts the timer to press the other trigger
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
        currentParticles = null;
    }

    private void SetWallPosition(ControllerArc arc, ControllerArc otherArc)
    {
        // Gets the two arc end positions
        Vector3 thisArcPos = arc.GetEndPosition();
        Vector3 otherArcPos = otherArc.GetEndPosition();

        // Calculates the position fo the wall
        float wallPosX = (thisArcPos.x + otherArcPos.x) / 2;
        float wallPosY = Math.Min(thisArcPos.y, otherArcPos.y);
        float wallPosZ = (thisArcPos.z + otherArcPos.z) / 2;
        wallOutline.transform.position = new Vector3(wallPosX, wallPosY, wallPosZ);

        // Corrects the height of the wall based on the mesh size and distance from the ground that the center is
        MeshRenderer meshRenderer = wallPrefab.GetComponentInChildren<MeshRenderer>();
        float verticleCorrection = PlayerAbility.CalculateOutlineVerticleCorrection(wallOutline, outlineLayerMask, out bool outOfBounds);
        verticleCorrection += wallMaxHeight - meshRenderer.bounds.size.y;
        wallOutline.transform.position += new Vector3(0, verticleCorrection, 0);
    }

    private void SetWallLocation(ControllerArc arc, ControllerArc otherArc)
    {
        SetWallPosition(arc, otherArc);

        MeshRenderer meshRenderer = wallPrefab.GetComponentInChildren<MeshRenderer>();

        // Calculates the size of the wall based on the distance between the player's hands
        float remainingEnergy = playerEnergy.GetRemainingEnergy();
        float maxHandDist = remainingEnergy / (wallSizeMultiplier * wallMaxHeight);
        float handDistance = (arc.GetEndPointsDistance(otherArc) < maxHandDist) ?
            arc.GetEndPointsDistance(otherArc) :
            maxHandDist;
        float wallWidth = ((handDistance - meshRenderer.bounds.size.x) / meshRenderer.bounds.size.x) + 1;;
        wallOutline.transform.localScale = new Vector3(wallWidth, wallOutline.transform.localScale.y, wallOutline.transform.localScale.z);

        // Calculates the angle that the wall needs to turn in order to match the rotation of the player's hands
        float angle = Vector3.SignedAngle(arc.GetEndPosition() - otherArc.GetEndPosition(), wallOutline.transform.position, new Vector3(0, -1, 0));
        angle += Vector3.SignedAngle(wallOutline.transform.position, new Vector3(1, 0, 0), new Vector3(0, -1, 0));
        float newAngle = angle;
        angle -= lastAngle;
        if (Math.Abs(angle) >= 0.5f)
        {
            // Rotates after enough change has occurred to prevent jittering
            lastAngle = newAngle;
            wallOutline.transform.Rotate(0, angle, 0, Space.Self);
        }
    }

    private bool WallIsValid(ControllerArc arc, ControllerArc otherArc)
    {
        // Wall is valid when both arcs are valid, there's no collision, and the wall is outside the player radius
        Vector3 playerFeetPos = new Vector3(player.transform.position.x, wallOutline.transform.position.y, player.transform.position.z);
        OutlineProperties properties = wallOutline.GetComponentInChildren<OutlineProperties>();
        return (arc.CanUseAbility() &&
            otherArc.CanUseAbility() &&
            !properties.CollisionDetected() &&
            Vector3.Distance(playerFeetPos, wallOutline.transform.position) >= rockCreationDistance);
    }

    public bool WallIsActive()
    {
        return wall != null;
    }

    public bool WallOutlineIsActive()
    {
        return wallOutline != null;
    }

    public bool OneHandHeld()
    {
        return firstHandHeld != null;
    }
}