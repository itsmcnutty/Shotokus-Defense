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

    public float spikeMinHandMovement = 0.05f;
    public float quicksandMinHandMovement = 0.05f;
    public float baseSpikeRadius = 0.5f;
    public float spikeSpeedReduction = 10f;
    public float spikeMinSpeed = .05f;
    public float spikeMaxHeight = 1.75f;
    public float energyPerSpikeInChain = 50;
    public float maxNumberOfSpikeChains = 5;
    public float maxSpikesInChain = 50;
    public float maxSpikeDiameter = 5f;
    public float quicksandSizeMultiplier = 2f;
    public float maxEarthquakeDistance = 3f;
    public float earthquakeDuration = 1f;

    public ParticleSystem createSpikeRockParticles;
    public ParticleSystem createSpikeEarthParticles;
    public ParticleSystem destroySpikeParticles;
    public ParticleSystem createQuicksandParticles;
    public ParticleSystem destroyQuicksandParticles;

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
        // Sets the position offsets for the spikes to create the hexagonal shape in circle mode
        spikeLocations.Add(new Vector2(2, 0));
        spikeLocations.Add(new Vector2(1, 1));
        spikeLocations.Add(new Vector2(-1, 1));
        spikeLocations.Add(new Vector2(-2, 0));
        spikeLocations.Add(new Vector2(-1, -1));
        spikeLocations.Add(new Vector2(1, -1));

        // Creates a number of spikes based on the number of possible layers in the circle
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
        // Creates an outline at the position of the ability arc
        ControllerArc arc = hand.GetComponentInChildren<ControllerArc>();
        GameObject spikeQuicksandOutline = Instantiate(areaOutlinePrefab);

        spikeQuicksandOutline.transform.position = arc.GetEndPosition();
        startingSpikeHandHeight = hand.transform.position.y;

        if (PlayerAbility.SpikeChainEnabled)
        {
            // Stores the direction that the spikes are facing at the time of creation
            Vector3 heading = spikeQuicksandOutline.transform.position - player.transform.position;

            float distance = heading.magnitude;
            Vector3 velocity = (heading / distance);

            MeshRenderer meshRenderer = spikeQuicksandOutline.GetComponentInChildren<MeshRenderer>();
            float distanceRatio = meshRenderer.bounds.size.x / 1;
            horizontalSpikeChainVelocity = new Vector2(velocity.x, velocity.z).normalized;
            horizontalSpikeChainVelocity *= distanceRatio;

            // Sets the base energy cost
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

        // Stores the previous 5 velocities of the hand for more responsive gameplay when creating spikes
        previousVelocities.Enqueue(controllerPose.GetVelocity().y);
        if (previousVelocities.Count > 5)
        {
            previousVelocities.Dequeue();
        }

        // Resizes the rings in a growing circular motion
        if (handDistance < 0 || !PlayerAbility.SpikeChainEnabled)
        {
            // Assures that only one outline exists (deletes any excess from the chain spike)
            GameObject spikeQuicksandOutline = spikeQuicksandOutlines[0];
            while (spikeQuicksandOutlines.Count > 1)
            {
                GameObject outline = spikeQuicksandOutlines[1];
                Destroy(outline);
                spikeQuicksandOutlines.Remove(outline);
            }

            // Calculates the new size and energy cost of the resized area
            Vector3 newSize = new Vector3();
            float energyCost = 0;
            if (handDistance < 0 && PlayerAbility.QuicksandAbilityEnabled)
            {
                newSize = new Vector3(size * quicksandSizeMultiplier, 1f, size * quicksandSizeMultiplier);
                energyCost = spikeQuicksandOutline.transform.localScale.x * playerEnergy.maxEnergy / (maxSpikeDiameter * quicksandSizeMultiplier);
            }
            else if(PlayerAbility.SpikeAbilityEnabled)
            {
                newSize = new Vector3(size, 1f, size);
                energyCost = spikeQuicksandOutline.transform.localScale.x * playerEnergy.maxEnergy / maxSpikeDiameter;
            }
            playerEnergy.SetTempEnergy(hand, energyCost);

            // Checks to make sure that this new area doesn't collide with an illegal object
            MeshRenderer meshRenderer = areaOutlinePrefab.GetComponentInChildren<MeshRenderer>();
            Collider[] colliders = Physics.OverlapSphere(spikeQuicksandOutline.transform.position, newSize.x * meshRenderer.bounds.size.x / 2, outlineLayerMask);
            bool collision = false;
            foreach (Collider collider in colliders)
            {
                if (collider.transform.root != spikeQuicksandOutline.transform && collider.tag != "Ground")
                {
                    collision = true;
                    break;
                }
            }

            // Sets the new size if no collision is found and the energy allows for it, or if the current area size is less than the original
            if ((!collision && playerEnergy.EnergyIsNotZero() && energyCost <= playerEnergy.maxEnergy) || newSize.x < spikeQuicksandOutline.transform.localScale.x)
            {
                spikeQuicksandOutline.transform.localScale = newSize;
            }

            // Sets the material to show the player if their ability area is valid or invalid
            PlayerAbility.SetOutlineMaterial(spikeQuicksandOutline, SpikeQuicksandIsValid(arc, spikeQuicksandOutline), validOutlineMat, invalidOutlineMat);
        }
        else
        {
            // Resets the position of the first ability area when it's the only one present
            if (spikeQuicksandOutlines.Count == 1)
            {
                spikeQuicksandOutlines[0].transform.localScale = areaOutlinePrefab.transform.localScale;
            }

            // Sets the outline material for all spike areas
            foreach (GameObject outline in spikeQuicksandOutlines)
            {
                PlayerAbility.SetOutlineMaterial(outline, SpikeQuicksandIsValid(arc, outline), validOutlineMat, invalidOutlineMat);
            }

            // Calculates the size of the outline and the positional offset for all new ability areas
            float outlineSize = baseSpikeRadius * 2;
            Vector2 spikeChainOffset = Vector2.Perpendicular(horizontalSpikeChainVelocity);

            Vector3 arcPos = arc.GetEndPosition();
            int numOutlines = 1;
            float energyCost = outlineSize * playerEnergy.maxEnergy / maxSpikeDiameter;

            // Creates as many outlines as the current area size allows for
            for (float i = outlineSize; i < (size - outlineSize); i += outlineSize)
            {
                numOutlines++;
                energyCost += energyPerSpikeInChain;
                
                // Checks that the new spike can be created
                if (CanMakeSpikeChain(spikeQuicksandOutlines, numOutlines, energyCost))
                {
                    // Offsets all previously created spikes to make room for the new one
                    CorrectSpikeChainOutline(spikeQuicksandOutlines, spikeChainOffset, true);

                    GameObject newOutline = Instantiate(areaOutlinePrefab) as GameObject;

                    // Calculates the position of the new outline
                    float posX = arcPos.x + (i * spikeChainOffset.x) - (spikeChainOffset.x * spikeQuicksandOutlines.Count) / 2;
                    float posZ = arcPos.z + (i * spikeChainOffset.y) - (spikeChainOffset.y * spikeQuicksandOutlines.Count) / 2;
                    
                    // Corrects the height of the new outline to be on the ground, starting from the previous outline's height
                    GameObject lastOutlinePlaced = spikeQuicksandOutlines[spikeQuicksandOutlines.Count - 1];
                    newOutline.transform.position = new Vector3(posX, lastOutlinePlaced.transform.position.y, posZ);
                    float verticleCorrection = PlayerAbility.CalculateOutlineVerticleCorrection(newOutline, outlineLayerMask, out bool outOfBounds);
                    newOutline.transform.position += new Vector3(0, verticleCorrection, 0);

                    spikeQuicksandOutlines.Add(newOutline);
                    playerEnergy.SetTempEnergy(hand, energyCost);
                }
            }

            while (numOutlines < spikeQuicksandOutlines.Count)
            {
                // Removes any spikes that are outside the area of effect
                GameObject removedOutline = spikeQuicksandOutlines[numOutlines];
                Destroy(removedOutline);
                spikeQuicksandOutlines.Remove(removedOutline);
                energyCost -= energyPerSpikeInChain;
                playerEnergy.SetTempEnergy(hand, energyCost);

                // Offsets all previously created spikes to take the place of the old one
                CorrectSpikeChainOutline(spikeQuicksandOutlines, spikeChainOffset, false);
            }
        }
        return spikeQuicksandOutlines;
    }

    private bool CanMakeSpikeChain(List<GameObject> spikeQuicksandOutlines, float numOutlines, float energyCost)
    {
        // Makes a new spike chain if there is a new outline, the max number of chains has not been reached, and the player has energy for it
        return (numOutlines > spikeQuicksandOutlines.Count && numOutlines <= maxNumberOfSpikeChains &&
            playerEnergy.EnergyIsNotZero() && energyCost <= playerEnergy.maxEnergy);
    }

    private void CorrectSpikeChainOutline(List<GameObject> spikeQuicksandOutlines, Vector3 spikeChainOffset, bool addSpike)
    {
        if (addSpike)
        {
            // Moves the spikes to the right if adding a new outline, otherwie moves left
            spikeChainOffset *= -1;
        }
        foreach (GameObject outline in spikeQuicksandOutlines)
        {
            // Repositions each outline by the offset
            Vector3 outlinePos = outline.transform.position;
            float correctionX = spikeChainOffset.x / 2;
            float correctionZ = spikeChainOffset.y / 2;
            outline.transform.position = new Vector3(outlinePos.x + correctionX, outlinePos.y, outlinePos.z + correctionZ);

            // Recalculates the height of each spike
            float verticleCorrection = PlayerAbility.CalculateOutlineVerticleCorrection(outline, outlineLayerMask, out bool outOfBounds);
            outline.transform.position += new Vector3(0, verticleCorrection, 0);
        }
    }

    public void TryCreateSpikesOrQuicksand(List<GameObject> spikeQuicksandOutlines, Hand hand, Hand otherHand, SteamVR_Behaviour_Pose controllerPose, float startingSpikeHandHeight, Vector2 horizontalSpikeChainVelocity)
    {
        // Calculates the height of the player's hands since the beginning of the ability and their velocity
        ControllerArc arc = hand.GetComponentInChildren<ControllerArc>();
        float controllerVelocity = previousVelocities.Average();
        float handPos = (hand.transform.position.y - startingSpikeHandHeight);
        if (handPos < -quicksandMinHandMovement && SpikeQuicksandIsValid(arc, spikeQuicksandOutlines[0]) && PlayerAbility.QuicksandAbilityEnabled)
        {
            // Creates quicksand if the player lowered their hands and the area is valid
            StartCoroutine(CreateQuicksand(spikeQuicksandOutlines, hand, otherHand));
        }
        else if (handPos > spikeMinHandMovement && controllerVelocity > 0 && PlayerAbility.SpikeAbilityEnabled)
        {
            // Creates spikes if the player raised their hands and they have upward velocity
            CreateSpikes(spikeQuicksandOutlines, hand, arc, controllerPose, controllerVelocity, horizontalSpikeChainVelocity);
        }
        else
        {
            // Destroys the outline(s) if the movements are invalid
            CancelSpikes(spikeQuicksandOutlines);
            playerEnergy.CancelEnergyUsage(hand);
        }
    }

    private IEnumerator CreateQuicksand(List<GameObject> spikeQuicksandOutlines, Hand hand, Hand otherHand)
    {
        GameObject spikeQuicksandOutline = spikeQuicksandOutlines[0];
        GameObject quicksand = Instantiate(quicksandPrefab) as GameObject;
        MeshRenderer outlineMeshRenderer = spikeQuicksandOutline.GetComponentInChildren<MeshRenderer>();
        MeshRenderer quicksandMeshRenderer = quicksandPrefab.GetComponentInChildren<MeshRenderer>();

        // Creates the quicksand .1 meters below the surface
        Vector3 outlinePos = spikeQuicksandOutline.transform.position;
        float yOffset = outlineMeshRenderer.bounds.max.y + 0.1f - outlinePos.y;
        quicksand.transform.position = new Vector3(outlinePos.x, outlinePos.y - yOffset, outlinePos.z);

        // Sizes the quicksand
        float quicksandSize = outlineMeshRenderer.bounds.size.x / quicksandMeshRenderer.bounds.size.x;
        quicksand.transform.localScale = new Vector3(quicksandSize, 1f, quicksandSize);

        // Plays the quicksand creation particle effect
        ParticleSystem particleSystem = Instantiate(createQuicksandParticles);
        particleSystem.transform.position = outlinePos;

        // Destroys the outline and uses the energy
        Destroy(spikeQuicksandOutline);
        spikeQuicksandOutlines.Remove(spikeQuicksandOutline);

        // Adds the quicksand component to begin the death countdown and perform the earthquake if active
        QuicksandProperties.CreateComponent(quicksand, maxEarthquakeDistance, earthquakeDuration,
            destroyQuicksandParticles);
        if (!PlayerAbility.EarthquakeEnabled)
        {
            // Increments the earthquake counter if it's not active
            float quicksandEnergy = playerEnergy.GetEnergyForHand(hand);
            PowerupController.IncrementEarthquakeCounter(quicksandEnergy);
            StartCoroutine(PlayerAbility.LongVibration(hand, 1f, 1500));
        }
        else
        {
            // Plays stronger haptic feedback during the earthquake
            StartCoroutine(PlayerAbility.LongVibration(hand, 1.5f, 2500));
            StartCoroutine(PlayerAbility.LongVibration(otherHand, 1.5f, 2500));
        }
        playerEnergy.UseEnergy(hand);

        // Wait .1 second (gives time for the particle effect to do something)
        yield return new WaitForSeconds(0.1f);
        float startTime = Time.time;
        float totalTime = 0;
        do
        {
            // Raises the quicksand to its final position
            totalTime += Time.deltaTime / 1f;
            if (!quicksand)
            {
                // Ends the loop if the player destroys the quicksand before it completes
                break;
            }
            quicksand.transform.position = Vector3.Lerp(quicksand.transform.position, outlinePos, totalTime);
            yield return new WaitForEndOfFrame();
        } while (totalTime <= 1f);
    }

    private void CreateSpikes(List<GameObject> spikeQuicksandOutlines, Hand hand, ControllerArc arc, SteamVR_Behaviour_Pose controllerPose, float controllerVelocity, Vector2 horizontalSpikeChainVelocity)
    {
        if (PlayerAbility.SpikeChainEnabled)
        {
            playerEnergy.UseEnergy(hand);
            float spikeVelocity = (controllerPose.GetVelocity().y / spikeSpeedReduction) + spikeMinSpeed;
            // Creates a Co-Routine for each spike chain so that they work independently from one-another
            while (spikeQuicksandOutlines.Count > 0)
            {
                GameObject outline = spikeQuicksandOutlines[0];
                StartCoroutine(CreateChainSpike(hand, outline, horizontalSpikeChainVelocity, spikeVelocity));
                spikeQuicksandOutlines.Remove(outline);
            }
        }
        else if (SpikeQuicksandIsValid(arc, spikeQuicksandOutlines[0]))
        {
            // Calculates the distance for creating the triangle of spikes
            GameObject spikeQuicksandOutline = spikeQuicksandOutlines[0];
            float finalSpikeRadius = baseSpikeRadius;
            float size = spikeQuicksandOutline.transform.localScale.x / 2;
            float triangleDist = (2 * baseSpikeRadius / (float) Math.Sqrt(3)) + baseSpikeRadius;
            if (size >= triangleDist && size < baseSpikeRadius * 3)
            {
                // Creates the locations for a triangle of spikes if the area size calls for it and return the maximum radius size to fill the area
                finalSpikeRadius = GenerateSpikesTriangle(spikeQuicksandOutline.transform.position, size, triangleDist);
            }
            else
            {
                // Creates the locations for a hexagon of spikes and returns the maximum radius size to fill the area
                float height = (float) Math.Sqrt(3) * baseSpikeRadius;
                finalSpikeRadius = GenerateSpikesHex(spikeQuicksandOutline.transform.position, spikeQuicksandOutline.transform.position, height, size);
            }
            float radiusIncrease = finalSpikeRadius - baseSpikeRadius;

            // Sets the radius to be the diameter with a .05 meter offset to give the spikes some space
            finalSpikeRadius = (finalSpikeRadius * 2) - 0.05f;
            Vector3 centerLoc = spikeQuicksandOutline.transform.position;

            Destroy(spikeQuicksandOutline);
            spikeQuicksandOutlines.Remove(spikeQuicksandOutline);
            playerEnergy.UseEnergy(hand);
            StartCoroutine(PlayerAbility.LongVibration(hand, .2f, 3500));

            // Creates all of the spikes returned from the hex or triangle calculation
            foreach (Vector3 spikePos in allSpikes)
            {
                GameObject spike = GetNewSpike();

                // Sets the position of the spike as calculated, correcting for the newly increased radius size and the size of the mesh
                Vector3 spikeCorrection = (spikePos - centerLoc) * 0.33f;
                Vector3 radiusCorrection = new Vector3(Math.Sign(spikeCorrection.x) * radiusIncrease, 0, Math.Sign(spikeCorrection.z) * radiusIncrease);
                spike.transform.position = (spikePos - spikeCorrection) + radiusCorrection;

                // Varies the height of the spikes randomly based on the layer in which they were created
                float layerNum = (float) Math.Floor(Vector3.Distance(spikePos, centerLoc) / (baseSpikeRadius * 2));
                float layerScale = (float) Math.Pow(.8, layerNum);
                float finalSpikeHeight = spikeMaxHeight * layerScale * UnityEngine.Random.Range(0.9f, 1f);
                spike.transform.localScale = new Vector3(finalSpikeRadius, finalSpikeHeight, finalSpikeRadius);

                // Sets the spikes to come out at the player's hand velocity to a position 2 meters above the surface
                float spikeVelocity = (controllerVelocity / spikeSpeedReduction) + spikeMinSpeed;
                Vector3 spikeEndPosition = spike.transform.position;
                spikeEndPosition.y += (finalSpikeHeight * spikeMaxHeight);

                // Plays the particle animation for creating spikes
                ParticleSystem rockParticleSystem = Instantiate(createSpikeRockParticles);
                rockParticleSystem.transform.position = spike.transform.position;
                rockParticleSystem.transform.localScale = spike.transform.localScale;

                // Adds the SpikeMovement component to the spike to start the death countdown once it reaches it's final height and play particles later in life
                SpikeMovement.CreateComponent(spike, spikeVelocity, spikeEndPosition, createSpikeEarthParticles, destroySpikeParticles);
            }
            // Removes all spike locations calculated
            allSpikes.Clear();
        }
        else
        {
            // Cancels the spike ability if it's not valid
            CancelSpikes(spikeQuicksandOutlines);
            playerEnergy.CancelEnergyUsage(hand);
        }
    }

    private IEnumerator CreateChainSpike(Hand hand, GameObject outline, Vector2 spikeMoveDirection, float spikeVelocity)
    {
        int numSpikes = 0;
        float verticleCorrection = 0;
        bool outOfBounds;
        while (true)
        {
            // Destroys the spike chain if it's invalid or has exceeded the maximum length of the chain
            if (!SpikeChainIsValid(outline) || numSpikes > maxSpikesInChain)
            {
                Destroy(outline);
                break;
            }

            // Gets a new spike from the stash
            GameObject spike = GetNewSpike();
            spike.transform.position = outline.transform.position;
            numSpikes++;

            // Resizes the spike to be a random height
            float finalSpikeHeight = spikeMaxHeight * UnityEngine.Random.Range(0.9f, 1f);
            spike.transform.localScale = new Vector3(baseSpikeRadius * 2, finalSpikeHeight, baseSpikeRadius * 2);

            // Sets the end position for the new spike
            Vector3 spikeEndPosition = spike.transform.position;
            spikeEndPosition.y += (finalSpikeHeight * spikeMaxHeight);

            // Adds the SpikeMovement component to the spike
            SpikeMovement.CreateComponent(spike, spikeVelocity, spikeEndPosition, createSpikeEarthParticles, destroySpikeParticles);
            StartCoroutine(PlayerAbility.LongVibration(hand, .05f, 2000));

            // Repositions the outline to the next spike location
            outline.transform.position += new Vector3(spikeMoveDirection.x, 0, spikeMoveDirection.y);

            // Calculates and corrects the height for the spike so that it moves to a new level when needed
            verticleCorrection = PlayerAbility.CalculateOutlineVerticleCorrection(outline, outlineLayerMask, out outOfBounds);
            if (outOfBounds)
            {
                // Ends the spike chain if it somehow leaves the map area
                Destroy(outline);
                break;
            }
            outline.transform.position += new Vector3(0, verticleCorrection, 0);
            
            // Delays the next spike being created by .1 seconds
            yield return new WaitForSeconds(0.1f);
        }
    }

    private float GenerateSpikesTriangle(Vector3 centerLoc, float areaRadius, float triangleDist)
    {
        // Calculates the position of the three triangle vertices based on the center of the triangle
        Vector3 vertex1 = new Vector3(centerLoc.x, centerLoc.y, centerLoc.z + ((float) Math.Sqrt(3) * baseSpikeRadius / 3));
        Vector3 vertex2 = new Vector3(centerLoc.x - (baseSpikeRadius / 2), centerLoc.y, centerLoc.z - ((float) Math.Sqrt(3) * baseSpikeRadius / 6));
        Vector3 vertex3 = new Vector3(centerLoc.x + (baseSpikeRadius / 2), centerLoc.y, centerLoc.z - ((float) Math.Sqrt(3) * baseSpikeRadius / 6));

        // Adds the vertices to the spike array and returns the maximum spike radius size
        allSpikes.Add(vertex1);
        allSpikes.Add(vertex2);
        allSpikes.Add(vertex3);
        return ((areaRadius - triangleDist) / 2) + baseSpikeRadius;
    }

    private float GenerateSpikesHex(Vector3 position, Vector3 centerLoc, float height, float areaRadius)
    {
        // Adds the current position of the hex to the array
        float radius = areaRadius;
        allSpikes.Add(position);

        // Loops through all 6 locations
        foreach (Vector2 locationOffset in spikeLocations)
        {
            // Calculates the new position of the spike
            float newX = position.x + (baseSpikeRadius * locationOffset.x);
            float newZ = position.z + (height * locationOffset.y);
            Vector3 newPos = new Vector3(newX, position.y, newZ);

            // Checks that the new location of the spike does not match an already created one
            if (!SpikeApproximatelyEqual(newPos))
            {
                // Checks that the spike is within the area of effect
                float currentDistance = Vector3.Distance(newPos, centerLoc) + baseSpikeRadius;
                if (currentDistance > areaRadius)
                {
                    // Returns the maximum radius to fill the remaining area with spike
                    float layerNum = (float) Math.Floor((areaRadius - baseSpikeRadius) / (baseSpikeRadius * 2));
                    return (layerNum != 0) ? (areaRadius - baseSpikeRadius) / (2 * layerNum) : areaRadius;
                }
                else
                {
                    // Tries to create the next ring of spikes
                    radius = GenerateSpikesHex(newPos, centerLoc, height, areaRadius);
                }
            }
        }
        return radius;
    }

    private bool SpikeApproximatelyEqual(Vector3 newPos)
    {
        // Returns true if any of the spikes are approximately equal to any other
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
        // Checks that the arc and outline are valid
        OutlineProperties properties = spikeQuicksandOutline.GetComponentInChildren<OutlineProperties>();
        return (arc.CanUseAbility() &&
            !properties.CollisionDetected());
    }

    private bool SpikeChainIsValid(GameObject spikeInChain)
    {
        // Checks that the outline in the chain is valid
        OutlineProperties properties = spikeInChain.GetComponentInChildren<OutlineProperties>();
        return (!properties.CollisionDetected());
    }

    private GameObject GetNewSpike()
    {
        GameObject spike;
        if (availableSpikes.Count != 0)
        {
            // Retrieves a spike from the stash if one is present
            spike = availableSpikes[0];
            spike.SetActive(true);
            availableSpikes.Remove(spike);
        }
        else
        {
            // Creates a new spike if none are available
            spike = Instantiate(spikePrefab) as GameObject;
        }
        return spike;
    }

    public void ClearSpikeQuicksandOutlines(List<GameObject> spikeQuicksandOutlines)
    {
        // Destroys all outlines and clears the array;
        foreach (GameObject outline in spikeQuicksandOutlines)
        {
            Destroy(outline);
        }
        spikeQuicksandOutlines.Clear();
    }

    public static void MakeSpikeAvailable(GameObject spike)
    {
        // Readds the spike to the stash
        availableSpikes.Add(spike);
    }
}