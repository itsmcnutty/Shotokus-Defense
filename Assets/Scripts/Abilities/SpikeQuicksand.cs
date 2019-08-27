﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class SpikeQuicksand : MonoBehaviour
{
    private GameObject spikePrefab;
    private GameObject quicksandPrefab;
    private GameObject areaOutlinePrefab;
    private PlayerEnergy playerEnergy;
    private Material validOutlineMat;
    private Material invalidOutlineMat;
    private float baseSpikeRadius;
    private float spikeSpeedReduction;
    private float spikeMinSpeed;
    private float spikeMaxHeight;
    private LayerMask outlineLayerMask;
    private float energyPerSpikeInChain;
    private float maxSpikesInChain;
    private float maxSpikeDiameter;
    private float quicksandSizeMultiplier;
    private float maxEarthquakeDistance;
    private float earthquakeDuration;

    private float startingSpikeHandHeight;
    private Vector2 horizontalSpikeChainVelocity;
    private List<GameObject> spikeQuicksandOutlines = new List<GameObject>();
    private static List<Vector2> spikeLocations = new List<Vector2>();
    private HashSet<Vector3> allSpikes = new HashSet<Vector3>();
    private static List<GameObject> availableSpikes = new List<GameObject>();

    public static SpikeQuicksand CreateComponent(GameObject gameObjectToAdd, GameObject spikePrefab, GameObject quicksandPrefab, GameObject areaOutlinePrefab,
        PlayerEnergy playerEnergy, Material validOutlineMat, Material invalidOutlineMat, float baseSpikeRadius, float spikeSpeedReduction, float spikeMinSpeed,
        float spikeMaxHeight, LayerMask outlineLayerMask, float energyPerSpikeInChain, float maxSpikesInChain, float maxSpikeDiameter, float quicksandSizeMultiplier,
        float earthquakeDuration, float maxEarthquakeDistance)
    {
        SpikeQuicksand spikes = gameObjectToAdd.AddComponent<SpikeQuicksand>();

        spikes.spikePrefab = spikePrefab;
        spikes.quicksandPrefab = quicksandPrefab;
        spikes.areaOutlinePrefab = areaOutlinePrefab;
        spikes.playerEnergy = playerEnergy;
        spikes.validOutlineMat = validOutlineMat;
        spikes.invalidOutlineMat = invalidOutlineMat;
        spikes.baseSpikeRadius = baseSpikeRadius;
        spikes.spikeSpeedReduction = spikeSpeedReduction;
        spikes.spikeMinSpeed = spikeMinSpeed;
        spikes.spikeMaxHeight = spikeMaxHeight;
        spikes.outlineLayerMask = outlineLayerMask;
        spikes.energyPerSpikeInChain = energyPerSpikeInChain;
        spikes.maxSpikesInChain = maxSpikesInChain;
        spikes.maxSpikeDiameter = maxSpikeDiameter;
        spikes.quicksandSizeMultiplier = quicksandSizeMultiplier;
        spikes.earthquakeDuration = earthquakeDuration;
        spikes.maxEarthquakeDistance = maxEarthquakeDistance;

        spikeLocations.Add(new Vector2(2, 0));
        spikeLocations.Add(new Vector2(1, 1));
        spikeLocations.Add(new Vector2(-1, 1));
        spikeLocations.Add(new Vector2(-2, 0));
        spikeLocations.Add(new Vector2(-1, -1));
        spikeLocations.Add(new Vector2(1, -1));

        float maxRadius = (float) Math.Sqrt(playerEnergy.maxEnergy / 50);
        int numLayers = (int) Math.Floor((maxRadius - baseSpikeRadius) / (baseSpikeRadius * 2));
        int numSpikes = numLayers * spikeLocations.Count + 1;

        for (int i = 0; i < numSpikes; i++)
        {
            GameObject spike = Instantiate(spikePrefab) as GameObject;
            spike.transform.position = new Vector3(0, -10, 0);
            spike.SetActive(false);
            MakeSpikeAvailable(spike);
        }

        return spikes;
    }

    public List<GameObject> IntializeOutline(Hand hand, GameObject player)
    {
        ControllerArc arc = hand.GetComponentInChildren<ControllerArc>();
        spikeQuicksandOutlines.Add(Instantiate(areaOutlinePrefab));
        spikeQuicksandOutlines[0].transform.position = arc.GetEndPosition();
        startingSpikeHandHeight = hand.transform.position.y;

        if (PlayerAbility.SpikeChainEnabled())
        {
            Vector3 heading = spikeQuicksandOutlines[0].transform.position - player.transform.position;

            float distance = heading.magnitude;
            Vector3 velocity = (heading / distance);
            horizontalSpikeChainVelocity = new Vector2(velocity.x, velocity.z).normalized;
            playerEnergy.SetTempEnergy(hand, baseSpikeRadius * 2 * playerEnergy.maxEnergy / maxSpikeDiameter);
        }
        return spikeQuicksandOutlines;
    }

    public List<GameObject> UpdateOutline(Hand hand)
    {
        ControllerArc arc = hand.GetComponentInChildren<ControllerArc>();
        float handDistance = hand.transform.position.y - startingSpikeHandHeight;
        float size = (float) Math.Pow((Math.Abs(handDistance)) + (baseSpikeRadius * 2), 3);
        if (handDistance < 0 || !PlayerAbility.SpikeChainEnabled())
        {
            GameObject spikeQuicksandOutline = spikeQuicksandOutlines[0];
            while (spikeQuicksandOutlines.Count > 1)
            {
                GameObject outline = spikeQuicksandOutlines[1];
                Destroy(outline);
                spikeQuicksandOutlines.Remove(outline);
            }

            Vector3 newSize;
            float energyCost;
            if (handDistance < 0)
            {
                newSize = new Vector3(size * quicksandSizeMultiplier, 1f, size * quicksandSizeMultiplier);
                energyCost = spikeQuicksandOutline.transform.localScale.x * playerEnergy.maxEnergy / (maxSpikeDiameter * quicksandSizeMultiplier);
            }
            else
            {
                newSize = new Vector3(size, 1f, size);
                energyCost = spikeQuicksandOutline.transform.localScale.x * playerEnergy.maxEnergy / maxSpikeDiameter;
            }
            playerEnergy.SetTempEnergy(hand, energyCost);
            if ((playerEnergy.EnergyIsNotZero() && energyCost <= playerEnergy.maxEnergy) || newSize.x < spikeQuicksandOutline.transform.localScale.x)
            {
                spikeQuicksandOutline.transform.localScale = newSize;
            }
            SetOutlineMaterial(spikeQuicksandOutline, SpikeQuicksandIsValid(arc, spikeQuicksandOutline));
        }
        else
        {
            foreach (GameObject outline in spikeQuicksandOutlines)
            {
                SetOutlineMaterial(outline, SpikeQuicksandIsValid(arc, outline));
            }

            float outlineSize = baseSpikeRadius * 2;
            Vector2 spikeChainOffset = Vector2.Perpendicular(horizontalSpikeChainVelocity);

            Vector3 arcPos = arc.GetEndPosition();
            int numOutlines = 1;
            float energyCost = outlineSize * playerEnergy.maxEnergy / maxSpikeDiameter;
            for (float i = outlineSize; i < (size - outlineSize); i += outlineSize)
            {
                numOutlines++;
                energyCost += energyPerSpikeInChain;
                if (numOutlines > spikeQuicksandOutlines.Count && playerEnergy.EnergyIsNotZero() && energyCost <= playerEnergy.maxEnergy)
                {
                    CorrectSpikeChainOutline(spikeChainOffset, true);

                    GameObject newOutline = Instantiate(areaOutlinePrefab) as GameObject;

                    float posX = arcPos.x + (i * spikeChainOffset.x) - (spikeChainOffset.x * spikeQuicksandOutlines.Count) / 2;
                    float posZ = arcPos.z + (i * spikeChainOffset.y) - (spikeChainOffset.y * spikeQuicksandOutlines.Count) / 2;
                    GameObject lastOutlinePlaced = spikeQuicksandOutlines[spikeQuicksandOutlines.Count - 1];

                    newOutline.transform.position = new Vector3(posX, lastOutlinePlaced.transform.position.y, posZ);
                    float verticleCorrection = CalculateOutlineVerticleCorrection(newOutline, out bool outOfBounds);
                    newOutline.transform.position += new Vector3(0, verticleCorrection, 0);

                    spikeQuicksandOutlines.Add(newOutline);
                    playerEnergy.SetTempEnergy(hand, energyCost);
                }
            }

            while (numOutlines < spikeQuicksandOutlines.Count)
            {
                GameObject removedOutline = spikeQuicksandOutlines[numOutlines];
                Destroy(removedOutline);
                spikeQuicksandOutlines.Remove(removedOutline);
                energyCost -= energyPerSpikeInChain;
                playerEnergy.SetTempEnergy(hand, energyCost);

                CorrectSpikeChainOutline(spikeChainOffset, false);
            }
        }
        return spikeQuicksandOutlines;
    }

    private void CorrectSpikeChainOutline(Vector3 spikeChainOffset, bool addSpike)
    {
        if (addSpike)
        {
            spikeChainOffset *= -1;
        }
        foreach (GameObject outline in spikeQuicksandOutlines)
        {
            Vector3 outlinePos = outline.transform.position;
            float correctionX = spikeChainOffset.x / 2;
            float correctionZ = spikeChainOffset.y / 2;
            outline.transform.position = new Vector3(outlinePos.x + correctionX, outlinePos.y, outlinePos.z + correctionZ);
            float verticleCorrection = CalculateOutlineVerticleCorrection(outline, out bool outOfBounds);
            outline.transform.position += new Vector3(0, verticleCorrection, 0);
        }
    }

    public void TryCreateSpikesOrQuicksand(Hand hand, SteamVR_Behaviour_Pose controllerPose)
    {
        ControllerArc arc = hand.GetComponentInChildren<ControllerArc>();
        float controllerVelocity = controllerPose.GetVelocity().y;
        float handPos = (hand.transform.position.y - startingSpikeHandHeight);
        bool allOutlinesValid = true;
        foreach (GameObject outline in spikeQuicksandOutlines)
        {
            if (!SpikeQuicksandIsValid(arc, outline))
            {
                allOutlinesValid = false;
            }
        }
        if (handPos < 0 && allOutlinesValid)
        {
            GameObject spikeQuicksandOutline = spikeQuicksandOutlines[0];
            GameObject quicksand = Instantiate(quicksandPrefab) as GameObject;
            quicksand.transform.position = spikeQuicksandOutline.transform.position;

            MeshRenderer outlineMeshRenderer = spikeQuicksandOutline.GetComponentInChildren<MeshRenderer>();
            MeshRenderer quicksandMeshRenderer = quicksandPrefab.GetComponentInChildren<MeshRenderer>();
            float quicksandSize = outlineMeshRenderer.bounds.size.x / quicksandMeshRenderer.bounds.size.x;
            quicksand.transform.localScale = new Vector3(quicksandSize, 1f, quicksandSize);

            QuicksandProperties.CreateComponent(quicksand, maxEarthquakeDistance, earthquakeDuration);
            Destroy(spikeQuicksandOutline);
            spikeQuicksandOutlines.Remove(spikeQuicksandOutline);
            playerEnergy.UseEnergy(hand);
            if (!PlayerAbility.EarthquakeEnabled())
            {
                PowerupController.IncrementEarthquakeCounter();
                hand.TriggerHapticPulse(800);
            }
        }
        else if (handPos > 0 && controllerVelocity > 0 && allOutlinesValid)
        {
            CreateSpikes(hand, controllerPose, controllerVelocity);
        }
        else
        {
            CancelSpikes();
            playerEnergy.CancelEnergyUsage(hand);
        }
    }

    public void CreateSpikes(Hand hand, SteamVR_Behaviour_Pose controllerPose, float controllerVelocity)
    {
        if (PlayerAbility.SpikeChainEnabled())
        {
            playerEnergy.UseEnergy(hand);
            float spikeVelocity = (controllerPose.GetVelocity().y / spikeSpeedReduction) + spikeMinSpeed;
            while (spikeQuicksandOutlines.Count > 0)
            {
                GameObject outline = spikeQuicksandOutlines[0];
                StartCoroutine(CreateChainSpike(hand, outline, horizontalSpikeChainVelocity, spikeVelocity));
                spikeQuicksandOutlines.Remove(outline);
            }
        }
        else
        {
            PowerupController.IncrementSpikeChainCounter();
            GameObject spikeQuicksandOutline = spikeQuicksandOutlines[0];
            float finalSpikeRadius = baseSpikeRadius;
            float size = spikeQuicksandOutline.transform.localScale.x / 2;
            float triangleDist = (2 * baseSpikeRadius / (float) Math.Sqrt(3)) + baseSpikeRadius;
            if (size >= triangleDist && size < baseSpikeRadius * 3)
            {
                finalSpikeRadius = GenerateSpikesTriangle(spikeQuicksandOutline.transform.position, size, triangleDist);
            }
            else
            {
                float height = (float) Math.Sqrt(3) * baseSpikeRadius;
                finalSpikeRadius = GenerateSpikesHex(spikeQuicksandOutline.transform.position, spikeQuicksandOutline.transform.position, height, size);
            }
            float radiusIncrease = finalSpikeRadius - baseSpikeRadius;

            finalSpikeRadius = (finalSpikeRadius * 2) - 0.05f;
            Vector3 centerLoc = spikeQuicksandOutline.transform.position;

            Destroy(spikeQuicksandOutline);
            spikeQuicksandOutlines.Remove(spikeQuicksandOutline);
            playerEnergy.UseEnergy(hand);

            foreach (Vector3 spikePos in allSpikes)
            {
                GameObject spike = GetNewSpike();

                Vector3 spikeCorrection = (spikePos - centerLoc) * 0.33f;
                Vector3 radiusCorrection = new Vector3(Math.Sign(spikeCorrection.x) * radiusIncrease, 0, Math.Sign(spikeCorrection.z) * radiusIncrease);
                spike.transform.position = (spikePos - spikeCorrection) + radiusCorrection;

                float layerNum = (float) Math.Floor(Vector3.Distance(spikePos, centerLoc) / (baseSpikeRadius * 2));
                float layerScale = (float) Math.Pow(.8, layerNum);
                float finalSpikeHeight = spikeMaxHeight * layerScale * UnityEngine.Random.Range(0.9f, 1f);
                spike.transform.localScale = new Vector3(finalSpikeRadius, finalSpikeHeight, finalSpikeRadius);

                float spikeVelocity = (controllerVelocity / spikeSpeedReduction) + spikeMinSpeed;
                Vector3 spikeEndPosition = spike.transform.position;
                spikeEndPosition.y += (finalSpikeHeight * spikeMaxHeight);

                SpikeMovement.CreateComponent(spike, spikeVelocity, spikeEndPosition);
                hand.TriggerHapticPulse(1500);
            }
            allSpikes.Clear();
        }
    }

    private IEnumerator CreateChainSpike(Hand hand, GameObject outline, Vector2 spikeMoveDirection, float spikeVelocity)
    {
        int numSpikes = 0;
        float verticleCorrection = 0;
        while (true)
        {
            verticleCorrection = 0;
            GameObject spike = GetNewSpike();
            spike.transform.position = outline.transform.position;
            numSpikes++;

            float finalSpikeHeight = spikeMaxHeight * UnityEngine.Random.Range(0.9f, 1f);
            spike.transform.localScale = new Vector3(baseSpikeRadius * 2, finalSpikeHeight, baseSpikeRadius * 2);

            Vector3 spikeEndPosition = spike.transform.position;
            spikeEndPosition.y += (finalSpikeHeight * spikeMaxHeight);

            SpikeMovement.CreateComponent(spike, spikeVelocity, spikeEndPosition);
            hand.TriggerHapticPulse(1500);

            outline.transform.position += new Vector3(spikeMoveDirection.x, 0, spikeMoveDirection.y);

            bool outOfBounds;
            verticleCorrection = CalculateOutlineVerticleCorrection(outline, out outOfBounds);
            outline.transform.position += new Vector3(0, verticleCorrection, 0);
            if (!SpikeChainIsValid(outline) || numSpikes > maxSpikesInChain || outOfBounds)
            {
                Destroy(outline);
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
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

    private float GenerateSpikesTriangle(Vector3 centerLoc, float areaRadius, float triangleDist)
    {
        Vector3 vertex1 = new Vector3(centerLoc.x, centerLoc.y, centerLoc.z + ((float) Math.Sqrt(3) * baseSpikeRadius / 3));
        Vector3 vertex2 = new Vector3(centerLoc.x - (baseSpikeRadius / 2), centerLoc.y, centerLoc.z - ((float) Math.Sqrt(3) * baseSpikeRadius / 6));
        Vector3 vertex3 = new Vector3(centerLoc.x + (baseSpikeRadius / 2), centerLoc.y, centerLoc.z - ((float) Math.Sqrt(3) * baseSpikeRadius / 6));

        allSpikes.Add(vertex1);
        allSpikes.Add(vertex2);
        allSpikes.Add(vertex3);
        return ((areaRadius - triangleDist) / 2) + baseSpikeRadius;
    }

    private float GenerateSpikesHex(Vector3 position, Vector3 centerLoc, float height, float areaRadius)
    {
        float radius = areaRadius;
        allSpikes.Add(position);
        foreach (Vector2 locationOffset in spikeLocations)
        {
            float newX = position.x + (baseSpikeRadius * locationOffset.x);
            float newZ = position.z + (height * locationOffset.y);
            float newY = position.y;
            Vector3 newPos = new Vector3(newX, newY, newZ);
            if (!SpikeApproximatelyEqual(newPos))
            {
                float currentDistance = Vector3.Distance(newPos, centerLoc) + baseSpikeRadius;
                if (currentDistance > areaRadius)
                {
                    float layerNum = (float) Math.Floor((areaRadius - baseSpikeRadius) / (baseSpikeRadius * 2));
                    return (layerNum != 0) ? (areaRadius - baseSpikeRadius) / (2 * layerNum) : areaRadius;
                }
                else
                {
                    radius = GenerateSpikesHex(newPos, centerLoc, height, areaRadius);
                }
            }
        }
        return radius;
    }

    private bool SpikeApproximatelyEqual(Vector3 newPos)
    {
        foreach (Vector3 spike in allSpikes)
        {
            if (Vector3.SqrMagnitude(spike - newPos) < 0.0001)
            {
                return true;
            }
        }
        return false;
    }

    public void CancelSpikes()
    {
        ClearSpikeQuicksandOutlines();
    }

    private bool SpikeQuicksandIsValid(ControllerArc arc, GameObject spikeQuicksandOutline)
    {
        OutlineProperties properties = spikeQuicksandOutline.GetComponentInChildren<OutlineProperties>();
        return (arc.CanUseAbility() &&
            !properties.CollisionDetected());
    }

    private bool SpikeChainIsValid(GameObject spikeInChain)
    {
        OutlineProperties properties = spikeInChain.GetComponentInChildren<OutlineProperties>();
        return (!properties.CollisionDetected());
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

    private GameObject GetNewSpike()
    {
        GameObject spike;
        if (availableSpikes.Count != 0)
        {
            spike = availableSpikes[0];
            spike.SetActive(true);
            availableSpikes.Remove(spike);
        }
        else
        {
            spike = Instantiate(spikePrefab) as GameObject;
        }
        return spike;
    }

    private void ClearSpikeQuicksandOutlines()
    {
        foreach (GameObject outline in spikeQuicksandOutlines)
        {
            Destroy(outline);
        }
        spikeQuicksandOutlines.Clear();
    }

    public static void MakeSpikeAvailable(GameObject spike)
    {
        availableSpikes.Add(spike);
    }

    public bool SpikeQuicksandIsActive()
    {
        return spikeQuicksandOutlines.Count != 0;
    }
}