using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Rocks : MonoBehaviour
{
    public AudioSource rockThrow;
    public AudioSource rockHit;
    public GameObject rockPrefab;
    public float numberOfRocksInCluster = 4;
    public float minRockDiameter = 0.25f;
    public float maxRockDimater = 1.5f;
    public float rockMassScale = 100f;

    private PlayerEnergy playerEnergy;
    private static List<GameObject> availableRocks = new List<GameObject>();

    public static Rocks CreateComponent(GameObject player, PlayerEnergy playerEnergy)
    {
        Rocks rocks = player.GetComponent<Rocks>();
        rocks.playerEnergy = playerEnergy;
        return rocks;
    }

    public void InitRocks()
    {
        float numRocks = (numberOfRocksInCluster + 1) * RockProperties.GetRockLifetime() * 25;

        for (int i = 0; i < numRocks; i++)
        {
            GameObject rock = Instantiate(rockPrefab) as GameObject;
            rock.transform.position = new Vector3(0, -10, 0);
            rock.SetActive(false);
            MakeRockAvailable(rock);
        }
    }

    public GameObject PickupRock(Hand hand, Hand otherHand)
    {
        GameObject activeRock = null;
        if (otherHand.currentAttachedObject == null)
        {
            if (GetRockEnergyCost(hand.hoveringInteractable.gameObject) < playerEnergy.GetRemainingEnergy())
            {
                activeRock = hand.hoveringInteractable.gameObject;
                Destroy(activeRock.GetComponent<RockProperties>());
            }
            else
            {
                hand.hoveringInteractable = null;
            }
        }
        else if (hand.hoveringInteractable.gameObject != otherHand.currentAttachedObject && GetRockEnergyCost(hand.hoveringInteractable.gameObject) < playerEnergy.GetRemainingEnergy())
        {
            activeRock = hand.hoveringInteractable.gameObject;
            Destroy(activeRock.GetComponent<RockProperties>());
        }
        return activeRock;
    }

    public GameObject CreateNewRock(Hand hand, ControllerArc arc)
    {
        GameObject activeRock = GetNewRock();
        activeRock.transform.position = new Vector3(arc.GetEndPosition().x, arc.GetEndPosition().y - 0.25f, arc.GetEndPosition().z);
        hand.AttachObject(activeRock, GrabTypes.Scripted);
        return activeRock;
    }

    public void UpdateRock(GameObject activeRock, Hand hand)
    {
        float rockEnergyCost = GetRockEnergyCost(activeRock);
        rockEnergyCost = (rockEnergyCost < 0) ? 0 : rockEnergyCost;
        activeRock.GetComponent<Rigidbody>().mass = rockMassScale * activeRock.transform.localScale.x;
        playerEnergy.SetTempEnergy(hand, rockEnergyCost);
        hand.SetAllowResize(playerEnergy.GetRemainingEnergy() > 0);
    }

    public void ThrowRock(GameObject activeRock, Hand hand, Hand otherHand)
    {
        hand.DetachObject(activeRock);
        hand.SetAllowResize(true);
        if (otherHand.currentAttachedObject == activeRock)
        {
            float rockSize = (float) Math.Pow(Math.Floor(activeRock.transform.localScale.x * activeRock.transform.localScale.y * activeRock.transform.localScale.z), 3);
            playerEnergy.SetTempEnergy(hand, rockSize);
            playerEnergy.TransferHandEnergy(hand, otherHand);
            otherHand.GetComponent<PlayerAbility>().activeRock = activeRock;
        }
        else
        {
            activeRock.AddComponent<RockProperties>();
            activeRock.GetComponent<Rigidbody>().mass = rockMassScale * activeRock.transform.localScale.x;
            playerEnergy.UseEnergy(hand);
            hand.TriggerHapticPulse(500);

            Vector3 velocity, angularVelocity;
            activeRock.GetComponent<Throwable>().GetReleaseVelocities(hand, out velocity, out angularVelocity);

            rockThrow.PlayOneShot(rockThrow.clip);

            if (PlayerAbility.RockClusterEnabled())
            {
                if (velocity != Vector3.zero || angularVelocity != Vector3.zero)
                {
                    for (int i = 0; i < numberOfRocksInCluster; i++)
                    {
                        GameObject newRock = GetNewRock();
                        newRock.AddComponent<RockProperties>();
                        Rigidbody newRockRigidbody = newRock.GetComponent<Rigidbody>();

                        newRock.transform.position = activeRock.transform.position;
                        newRock.transform.localScale = activeRock.transform.localScale;

                        newRockRigidbody.velocity = velocity;
                        newRockRigidbody.velocity = Vector3.ProjectOnPlane(UnityEngine.Random.insideUnitSphere, velocity) * (.75f + activeRock.transform.localScale.x) + velocity;
                        newRockRigidbody.angularVelocity = newRock.transform.forward * angularVelocity.magnitude;
                        
                        rockThrow.PlayOneShot(rockThrow.clip);
                    }
                }
            }
            else
            {
                PowerupController.IncrementRockClusterCounter();
            }
        }
    }

    private float GetRockEnergyCost(GameObject rock)
    {
        float range = maxRockDimater - minRockDiameter;
        return (rock.transform.localScale.x - minRockDiameter) * playerEnergy.maxEnergy / range;
    }

    private GameObject GetNewRock()
    {
        GameObject newRock;
        if (availableRocks.Count != 0)
        {
            newRock = availableRocks[0];
            newRock.SetActive(true);
            availableRocks.Remove(newRock);
        }
        else
        {
            newRock = Instantiate(rockPrefab) as GameObject;
        }
        return newRock;
    }

    public static void MakeRockAvailable(GameObject rock)
    {
        availableRocks.Add(rock);
    }
}