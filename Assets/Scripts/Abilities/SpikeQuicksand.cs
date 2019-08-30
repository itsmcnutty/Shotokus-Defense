using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class SpikeQuicksand : MonoBehaviour
{
    public GameObject spikePrefab;
    public GameObject quicksandPrefab;
    public float baseSpikeRadius = 0.5f;
    public float spikeSpeedReduction = 10f;
    public float spikeMinSpeed = .05f;
    public float spikeMaxHeight = 1.75f;
    public float energyPerSpikeInChain = 50;
    public float maxSpikesInChain = 50;
    public float maxSpikeDiameter = 5f;
    public float quicksandSizeMultiplier = 2f;
    public float maxEarthquakeDistance = 3f;
    public float earthquakeDuration = 1f;

    [Header("Audio")]
    public AudioSource raiseSpike;
    public AudioSource breakSpike;

    private GameObject areaOutlinePrefab;
    private PlayerEnergy playerEnergy;
    private Material validOutlineMat;
    private Material invalidOutlineMat;
    private LayerMask outlineLayerMask;

    private static List<Vector2> spikeLocations = new List<Vector2>();
    private HashSet<Vector3> allSpikes = new HashSet<Vector3>();
    private Queue<float> previousVelocities = new Queue<float>();
    private static List<GameObject> availableSpikes = new List<GameObject>();

    public static SpikeQuicksand CreateComponent(GameObject player, GameObject areaOutlinePrefab, PlayerEnergy playerEnergy, Material validOutlineMat,
    Material invalidOutlineMat, LayerMask outlineLayerMask)
    {
        SpikeQuicksand spikes = player.GetComponent<SpikeQuicksand>();

        spikes.areaOutlinePrefab = areaOutlinePrefab;
        spikes.playerEnergy = playerEnergy;
        spikes.validOutlineMat = validOutlineMat;
        spikes.invalidOutlineMat = invalidOutlineMat;
        spikes.outlineLayerMask = outlineLayerMask;

        return spikes;
    }

    public void InitSpikes()
    {
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
    }

    public GameObject IntializeOutline(Hand hand, GameObject player, out float startingSpikeHandHeight, out Vector2 horizontalSpikeChainVelocity)
    {
        ControllerArc arc = hand.GetComponentInChildren<ControllerArc>();
        GameObject spikeQuicksandOutline = Instantiate(areaOutlinePrefab);

        spikeQuicksandOutline.transform.position = arc.GetEndPosition();
        startingSpikeHandHeight = hand.transform.position.y;

        if (PlayerAbility.SpikeChainEnabled())
        {
            Vector3 heading = spikeQuicksandOutline.transform.position - player.transform.position;

            float distance = heading.magnitude;
            Vector3 velocity = (heading / distance);

            MeshRenderer meshRenderer = spikeQuicksandOutline.GetComponentInChildren<MeshRenderer>();
            float distanceRatio = meshRenderer.bounds.size.x / 1;
            horizontalSpikeChainVelocity = new Vector2(velocity.x, velocity.z).normalized;
            horizontalSpikeChainVelocity *= distanceRatio;

            playerEnergy.SetTempEnergy(hand, baseSpikeRadius * 2 * playerEnergy.maxEnergy / maxSpikeDiameter);
        }
        else
        {
            horizontalSpikeChainVelocity = Vector2.zero;
        }
        return spikeQuicksandOutline;
    }

    public List<GameObject> UpdateOutline(List<GameObject> spikeQuicksandOutlines, Hand hand, SteamVR_Behaviour_Pose controllerPose, float startingSpikeHandHeight, Vector2 horizontalSpikeChainVelocity)
    {
        ControllerArc arc = hand.GetComponentInChildren<ControllerArc>();
        float handDistance = hand.transform.position.y - startingSpikeHandHeight;
        float size = (float) Math.Pow((Math.Abs(handDistance)) + (baseSpikeRadius * 2), 3);
        previousVelocities.Enqueue(controllerPose.GetVelocity().y);
        if (previousVelocities.Count > 5)
        {
            previousVelocities.Dequeue();
        }

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

            MeshRenderer meshRenderer = areaOutlinePrefab.GetComponentInChildren<MeshRenderer>();
            Collider[] colliders = Physics.OverlapSphere(spikeQuicksandOutline.transform.position, newSize.x * meshRenderer.bounds.size.x / 2, outlineLayerMask);
            bool collision = false;
            foreach(Collider collider in colliders)
            {
                if(collider.transform.root != spikeQuicksandOutline.transform && collider.tag != "Ground")
                {
                    collision = true;
                    break;
                }
            }

            if ((!collision && playerEnergy.EnergyIsNotZero() && energyCost <= playerEnergy.maxEnergy) || newSize.x < spikeQuicksandOutline.transform.localScale.x)
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
                    CorrectSpikeChainOutline(spikeQuicksandOutlines, spikeChainOffset, true);

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

                CorrectSpikeChainOutline(spikeQuicksandOutlines, spikeChainOffset, false);
            }
        }
        return spikeQuicksandOutlines;
    }

    private void CorrectSpikeChainOutline(List<GameObject> spikeQuicksandOutlines, Vector3 spikeChainOffset, bool addSpike)
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

    public void TryCreateSpikesOrQuicksand(List<GameObject> spikeQuicksandOutlines, Hand hand, SteamVR_Behaviour_Pose controllerPose, float startingSpikeHandHeight, Vector2 horizontalSpikeChainVelocity)
    {
        ControllerArc arc = hand.GetComponentInChildren<ControllerArc>();
        float controllerVelocity = previousVelocities.Average();
        float handPos = (hand.transform.position.y - startingSpikeHandHeight);
        if (handPos < 0 && SpikeQuicksandIsValid(arc, spikeQuicksandOutlines[0]))
        {
            GameObject spikeQuicksandOutline = spikeQuicksandOutlines[0];
            GameObject quicksand = Instantiate(quicksandPrefab) as GameObject;
            quicksand.transform.position = spikeQuicksandOutline.transform.position;

            MeshRenderer outlineMeshRenderer = spikeQuicksandOutline.GetComponentInChildren<MeshRenderer>();
            MeshRenderer quicksandMeshRenderer = quicksandPrefab.GetComponentInChildren<MeshRenderer>();
            float quicksandSize = outlineMeshRenderer.bounds.size.x / quicksandMeshRenderer.bounds.size.x;
            quicksand.transform.localScale = new Vector3(quicksandSize, 1f, quicksandSize);

            QuicksandProperties.CreateComponent(quicksand, maxEarthquakeDistance, earthquakeDuration, breakSpike);
            Destroy(spikeQuicksandOutline);
            spikeQuicksandOutlines.Remove(spikeQuicksandOutline);
            playerEnergy.UseEnergy(hand);
            if (!PlayerAbility.EarthquakeEnabled())
            {
                PowerupController.IncrementEarthquakeCounter();
                hand.TriggerHapticPulse(800);
            }
        }
        else if (handPos > 0 && controllerVelocity > 0)
        {
            CreateSpikes(spikeQuicksandOutlines, hand, arc, controllerPose, controllerVelocity, horizontalSpikeChainVelocity);
        }
        else
        {
            CancelSpikes(spikeQuicksandOutlines);
            playerEnergy.CancelEnergyUsage(hand);
        }
    }

    public void CreateSpikes(List<GameObject> spikeQuicksandOutlines, Hand hand, ControllerArc arc, SteamVR_Behaviour_Pose controllerPose, float controllerVelocity, Vector2 horizontalSpikeChainVelocity)
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
        else if(SpikeQuicksandIsValid(arc, spikeQuicksandOutlines[0]))
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
                raiseSpike.PlayOneShot(raiseSpike.clip);
            }
            allSpikes.Clear();
        }
        else
        {
            CancelSpikes(spikeQuicksandOutlines);
            playerEnergy.CancelEnergyUsage(hand);
        }
    }

    private IEnumerator CreateChainSpike(Hand hand, GameObject outline, Vector2 spikeMoveDirection, float spikeVelocity)
    {
        int numSpikes = 0;
        float verticleCorrection = 0;
        while (true)
        {
            if (!SpikeChainIsValid(outline) || numSpikes > maxSpikesInChain)
            {
                Destroy(outline);
                break;
            }
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
            raiseSpike.PlayOneShot(raiseSpike.clip);

            outline.transform.position += new Vector3(spikeMoveDirection.x, 0, spikeMoveDirection.y);

            bool outOfBounds;
            verticleCorrection = CalculateOutlineVerticleCorrection(outline, out outOfBounds);
            if (outOfBounds)
            {
                Destroy(outline);
                break;
            }
            outline.transform.position += new Vector3(0, verticleCorrection, 0);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private float CalculateOutlineVerticleCorrection(GameObject outline, out bool outOfBounds)
    {
        float verticleCorrection = 0;
        RaycastHit hit;
        if (Physics.Raycast(outline.transform.position + Vector3.up, Vector3.down, out hit, 2f, outlineLayerMask) ||
            Physics.Raycast(outline.transform.position, Vector3.down, out hit, 2f, outlineLayerMask))
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

    public void CancelSpikes(List<GameObject> spikeQuicksandOutlines)
    {
        ClearSpikeQuicksandOutlines(spikeQuicksandOutlines);
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

    public void ClearSpikeQuicksandOutlines(List<GameObject> spikeQuicksandOutlines)
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
}