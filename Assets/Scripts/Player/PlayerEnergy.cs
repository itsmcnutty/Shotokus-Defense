using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class PlayerEnergy : MonoBehaviour
{
    public Slider energyBarBefore;
    public Slider energyBarAfter;
    public Text energyBarText;
    public float maxEnergy;
    public float energyRegenPerSecond;
    public float regenDelayInSec;
    public bool debugShowEnergyText = false;

    private float currentEnergy;
    private float lastAbilityUsedTime;
    private Dictionary<Hand, float> activeAbilityEnergyCost;
    private bool infiniteEnergyDisabled = true;

    // Start is called before the first frame update
    void Start ()
    {
        activeAbilityEnergyCost = new Dictionary<Hand, float> ();
        currentEnergy = maxEnergy;
        energyBarBefore.maxValue = maxEnergy;
        energyBarBefore.value = maxEnergy;
        energyBarAfter.maxValue = maxEnergy;
        energyBarAfter.value = maxEnergy;
    }

    // Update is called once per frame
    void Update ()
    {
        RegenEnergy ();
        SetEnergyBarText ();
    }

    public void SetTempEnergy (Hand activeHand, float energy)
    {
        // Uses energy if not in infinite energy mode
        if (infiniteEnergyDisabled)
        {
            float lastEnergyVal;
            if (!activeAbilityEnergyCost.TryGetValue (activeHand, out lastEnergyVal))
            {
                // Adds a new hand with the given value if one has not been registered yet
                activeAbilityEnergyCost.Add (activeHand, energy);
            }
            else
            {
                // Adds energy if the hand already exists
                activeAbilityEnergyCost[activeHand] = energy;
            }

            // Sets energy cost to be no more than the maximum cost
            float afterAbilityEnergy = GetTotalEnergyUsage ();
            if (afterAbilityEnergy > currentEnergy)
            {
                activeAbilityEnergyCost[activeHand] -= (afterAbilityEnergy - currentEnergy);
                afterAbilityEnergy = currentEnergy;
            }
            energyBarAfter.value = currentEnergy - afterAbilityEnergy;
        }
        UpdateAbilityUseTime ();
    }

    public void DrainRealEnergy (float energy)
    {
        // Checks that infinite energy is disabled
        if (infiniteEnergyDisabled)
        {
            // Subtracts the given value from the energy, stopping at 0
            if (currentEnergy > 0)
            {
                currentEnergy -= energy;
                if (currentEnergy < 0)
                {
                    currentEnergy = 0;
                }
                energyBarBefore.value = currentEnergy;
                energyBarAfter.value = currentEnergy;
            }
        }
        UpdateAbilityUseTime ();
    }

    public void UseEnergy (Hand activeHand)
    {
        // Checks that infinite energy is disabled and that the hand isn't null
        if (infiniteEnergyDisabled && activeHand != null)
        {
            // Subtracts the energy stored on the hand and removes it
            currentEnergy -= activeAbilityEnergyCost[activeHand];
            energyBarBefore.value = currentEnergy;
            activeAbilityEnergyCost[activeHand] = 0;
            RemoveHandFromActive (activeHand);
        }
    }

    public void CancelEnergyUsage (Hand activeHand)
    {
        if (infiniteEnergyDisabled && activeHand != null)
        {
            // Removes the hand and its energy
            energyBarAfter.value = currentEnergy;
            activeAbilityEnergyCost[activeHand] = 0;
            RemoveHandFromActive (activeHand);
        }
    }

    public bool EnergyIsNotZero ()
    {
        return (currentEnergy - GetTotalEnergyUsage ()) > 0;
    }

    public bool EnergyAboveThreshold (float threshold)
    {
        return (currentEnergy - GetTotalEnergyUsage ()) > threshold;
    }

    public void UpdateAbilityUseTime ()
    {
        lastAbilityUsedTime = Time.time;
    }

    public float GetRemainingEnergy ()
    {
        return currentEnergy - GetTotalEnergyUsage ();
    }

    public void TransferHandEnergy (Hand activeHand, Hand newHand)
    {
        // Moves the energy on one hand into the other hand
        if (infiniteEnergyDisabled)
        {
            float value = RemoveHandFromActive (activeHand);
            SetTempEnergy (newHand, value);
        }
    }

//    private void SetEnergyBarText ()
//    {
//        if(debugShowEnergyText)
//        {
//            energyBarText.text = Math.Floor (currentEnergy - GetTotalEnergyUsage ()) + " / " + maxEnergy;
//        }
//        else
//        {
//            energyBarText.text = "Energy Level";
//        }
//    }

    float deltaTime = 0;
    private void SetEnergyBarText()
    {
        if (debugShowEnergyText)
        {
            energyBarText.text = Math.Floor(currentEnergy - GetTotalEnergyUsage()) + " / " + maxEnergy;
        }
        else
        {
            //energyBarText.text = "Energy Level";
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            energyBarText.text = Mathf.Ceil(fps).ToString();
        }
    }

    private void RegenEnergy ()
    {
        // Checks that the regen delay has expired and that energy is not at its maximum
        if ((Time.time - lastAbilityUsedTime) > regenDelayInSec && currentEnergy < maxEnergy)
        {
            // Regens energy until it reaches the max, setting it back to max if it get exceeded
            currentEnergy += (energyRegenPerSecond * Time.deltaTime);
            if (currentEnergy > maxEnergy)
            {
                currentEnergy = maxEnergy;
            }
            energyBarBefore.value = currentEnergy;
            energyBarAfter.value = currentEnergy;
        }
    }

    private float RemoveHandFromActive (Hand activeHand)
    {
        // Takes the hand out of the dictionary if it exists in it
        float value;
        if (activeAbilityEnergyCost.TryGetValue (activeHand, out value))
        {
            activeAbilityEnergyCost.Remove (activeHand);
            return value;
        }
        return 0;
    }

    private float GetTotalEnergyUsage ()
    {
        // Adds up all energy calculated from the dictionary values
        float totalEnergy = 0;
        foreach (float abilityEnergyCost in activeAbilityEnergyCost.Values)
        {
            totalEnergy += abilityEnergyCost;
        }
        return totalEnergy;
    }

}