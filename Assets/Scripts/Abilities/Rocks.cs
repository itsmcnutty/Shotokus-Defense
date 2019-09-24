using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Rocks : MonoBehaviour
{
    public GameObject rockPrefab;
    public float numberOfRocksInCluster = 4;
    public float minRockDiameter = 0.25f;
    public float maxRockDimater = 1.5f;
    public float rockMassScale = 100f;
    public float maxRockEnergyCost = 200f;

    public ParticleSystem createRockParticles;
    public ParticleSystem destroyRockParticles;
    public ParticleSystem regrowRockParticles;
    public ParticleSystem regrowRockSwirl;

    private PlayerEnergy playerEnergy;
    private static Queue<GameObject> availableRocks = new Queue<GameObject>();

    private static ParticleSystem currentRegrowthParticleSystem;
    private static ParticleSystem currentRegrowthSwirlSystem;

    public static Rocks CreateComponent(GameObject player, PlayerEnergy playerEnergy)
    {
        Rocks rocks = player.GetComponent<Rocks>();
        rocks.playerEnergy = playerEnergy;
        return rocks;
    }

    public void InitRocks()
    {
        // Creates a number of rocks at the start of the game to keep on reserve to avoid lag at runtime
        float numRocks = (numberOfRocksInCluster + 1) * RockProperties.GetRockLifetime() * 10;

        for (int i = 0; i < numRocks; i++)
        {
            GameObject rock = Instantiate(rockPrefab) as GameObject;
            rock.transform.position = new Vector3(0, -10, 0);
            rock.SetActive(false);
            MakeRockAvailable(rock);
        }
    }

    public GameObject PickupRock(GameObject pickup, Hand hand, Hand otherHand)
    {
        GameObject activeRock = null;
        // Trying to pick up a new object (not resizing)
        if (otherHand.currentAttachedObject == null)
        {
            if (GetRockEnergyCost(pickup) < playerEnergy.GetRemainingEnergy())
            {
                // Grabs the rock if the player's energy allows for it
                activeRock = pickup;
                playerEnergy.SetTempEnergy(hand, GetRockEnergyCost(pickup));
                hand.SetAllowResize(playerEnergy.GetRemainingEnergy() > 0 && GetRockEnergyCost(activeRock) < maxRockEnergyCost);
                Destroy(activeRock.GetComponent<RockProperties>());
            }
            else
            {
                // Prevents pickup if the player doesn't have enough energy
                hand.DetachObject(pickup);
                hand.hoveringInteractable = null;
            }
        }
        else if (pickup != otherHand.currentAttachedObject && GetRockEnergyCost(pickup) < playerEnergy.GetRemainingEnergy())
        {
            // Pickups rock when the other hand is holding one
            activeRock = pickup;
            playerEnergy.SetTempEnergy(hand, GetRockEnergyCost(pickup));
            hand.SetAllowResize(playerEnergy.GetRemainingEnergy() > 0 && GetRockEnergyCost(activeRock) < maxRockEnergyCost);
            Destroy(activeRock.GetComponent<RockProperties>());
        }
        return activeRock;
    }

    public GameObject CreateNewRock(Hand hand, ControllerArc arc)
    {
        // Gets a rock from the stash and attaches it to the player's hand
        GameObject activeRock = GetNewRock();
        activeRock.transform.position = new Vector3(arc.GetEndPosition().x, arc.GetEndPosition().y - minRockDiameter, arc.GetEndPosition().z);
        activeRock.transform.localScale = new Vector3(minRockDiameter, minRockDiameter, minRockDiameter);
        activeRock.GetComponent<Rigidbody>().mass = rockMassScale * minRockDiameter;
        hand.AttachObject(activeRock, GrabTypes.Scripted);
        playerEnergy.SetTempEnergy(hand, 0);

        // Plays a particle effect at the point of picking up a rock
        ParticleSystem rockParticleSystem = Instantiate(createRockParticles);
        rockParticleSystem.transform.position = activeRock.transform.position;

        return activeRock;
    }

    public void UpdateRock(GameObject activeRock, Hand hand)
    {
        // Sets energy cost and mass of the rock
        float rockEnergyCost = GetRockEnergyCost(activeRock);
        rockEnergyCost = (rockEnergyCost < 0) ? 0 : rockEnergyCost;
        activeRock.GetComponent<Rigidbody>().mass = rockMassScale * activeRock.transform.localScale.x;
        playerEnergy.SetTempEnergy(hand, rockEnergyCost);

        // Prevents resizing if the player doesn't have enough energy
        hand.SetAllowResize(playerEnergy.GetRemainingEnergy() > 0 && rockEnergyCost < maxRockEnergyCost);

        if (currentRegrowthParticleSystem == null)
        {
            // Creates a new regrowth particle system if one does not exist
            currentRegrowthParticleSystem = Instantiate(regrowRockParticles);
            currentRegrowthSwirlSystem = Instantiate(regrowRockSwirl);
            currentRegrowthSwirlSystem.transform.parent = activeRock.transform;
        }

        // Sets position of the particle system to be at the rocks new position;
        SkinnedMeshRenderer skinnedMeshRenderer = activeRock.GetComponent<SkinnedMeshRenderer>();
        currentRegrowthParticleSystem.transform.position = skinnedMeshRenderer.bounds.center;
        currentRegrowthSwirlSystem.transform.position = skinnedMeshRenderer.bounds.center;
    }

    public void StopRegrowthParticles()
    {
        if (currentRegrowthParticleSystem != null)
        {
            // Stops the particle animations when the player is not longer resizing the rock
            UnityEngine.ParticleSystem.MainModule particleMain = currentRegrowthParticleSystem.main;
            particleMain.loop = false;
            currentRegrowthParticleSystem = null;

            UnityEngine.ParticleSystem.MainModule swirlMain = currentRegrowthSwirlSystem.main;
            swirlMain.loop = false;
            currentRegrowthSwirlSystem = null;
        }
    }

    public void ThrowRock(GameObject activeRock, Hand hand, Hand otherHand)
    {
        // Detaches the rock from the player's hand
        hand.DetachObject(activeRock);

        if (otherHand.currentAttachedObject == activeRock)
        {
            // Rebases the rock to the other hand if it's still holding the rock at the time of release
            float rockSize = (float) Math.Pow(Math.Floor(activeRock.transform.localScale.x * activeRock.transform.localScale.y * activeRock.transform.localScale.z), 3);
            playerEnergy.SetTempEnergy(hand, rockSize);
            playerEnergy.TransferHandEnergy(hand, otherHand);
            otherHand.GetComponent<PlayerAbility>().activeRock = activeRock;
        }
        else
        {
            hand.SetAllowResize(true);

            // Adds the RockProperties component to the rock to begin the death countdown
            RockProperties.CreateComponent(activeRock, destroyRockParticles);

            // Gets the final mass of the rock
            activeRock.GetComponent<Rigidbody>().mass = rockMassScale * activeRock.transform.localScale.x;
            playerEnergy.UseEnergy(hand);
            StartCoroutine(PlayerAbility.LongVibration(hand, 0.1f, 1000));

            // Uses power-up if enabled
            if (PlayerAbility.RockClusterEnabled)
            {
                // Gets the speed of the rock at the time of release
                Vector3 velocity, angularVelocity;
                activeRock.GetComponent<Throwable>().GetReleaseVelocities(hand, out velocity, out angularVelocity);

                if (velocity != Vector3.zero || angularVelocity != Vector3.zero)
                {
                    // Creates cluster of rocks when the rock is being thrown
                    for (int i = 0; i < numberOfRocksInCluster; i++)
                    {
                        // Gets a rock from the stash and adds the RockProperties component
                        GameObject newRock = GetNewRock();
                        RockProperties.CreateComponent(newRock, destroyRockParticles);

                        // Mimics the data from the original rock to create a new one
                        Rigidbody newRockRigidbody = newRock.GetComponent<Rigidbody>();
                        newRockRigidbody.mass = rockMassScale * activeRock.transform.localScale.x;

                        newRock.transform.position = activeRock.transform.position;
                        newRock.transform.localScale = activeRock.transform.localScale;

                        // Randomizes a direction of travel for the new rock in the cluster
                        newRockRigidbody.velocity = velocity;
                        newRockRigidbody.velocity = Vector3.ProjectOnPlane(UnityEngine.Random.insideUnitSphere, velocity) * (.75f + activeRock.transform.localScale.x) + velocity;
                        newRockRigidbody.angularVelocity = newRock.transform.forward * angularVelocity.magnitude;
                    }
                }
            }
        }
    }

    private float GetRockEnergyCost(GameObject rock)
    {
        // Calculates cost of the rock based on its minimum and maximum size and maximum energy cost
        float range = maxRockDimater - minRockDiameter;
        return (rock.transform.localScale.x - minRockDiameter) * maxRockEnergyCost / range;
    }

    private GameObject GetNewRock()
    {
        GameObject newRock;
        if (availableRocks.Count != 0)
        {       
            // Gets a rock from the stash if one is available
            newRock = availableRocks.Dequeue();
            newRock.SetActive(true);
        }
        else
        {        
            // Creates a new rock if one is not available from the stash
            newRock = Instantiate(rockPrefab) as GameObject;
        }
        return newRock;
    }

    public static void MakeRockAvailable(GameObject rock)
    {
        // Re-adds the rock to the stash for later usage
        availableRocks.Enqueue(rock);
    }
}