﻿using System.Collections;
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
    public float regenEnergyRate;
    public float regenDelayInSec;

    private float currentEnergy;
    private float lastAbilityUsedTime;
    private Dictionary<Hand, float> activeAbilityEnergyCost;

    // Start is called before the first frame update
    void Start ()
    {
        activeAbilityEnergyCost = new Dictionary<Hand, float> ();
        currentEnergy = maxEnergy;
        energyBarBefore.maxValue = maxEnergy;
        energyBarBefore.value = maxEnergy;
        energyBarAfter.maxValue = maxEnergy;
        energyBarAfter.value = maxEnergy;
        SetEnergyBarText ();
    }

    // Update is called once per frame
    void Update () {
        RegenEnergy ();
    }

    public void DrainTempEnergy (Hand activeHand, float energy)
    {
        if (EnergyIsNotZero ())
        {
            activeAbilityEnergyCost[activeHand] += energy;
            float afterAbilityEnergy = GetTotalEnergyUsage ();
            if (afterAbilityEnergy < 0)
            {
                afterAbilityEnergy = 0;
                activeAbilityEnergyCost[activeHand] -= (energy - afterAbilityEnergy);
            }
            energyBarAfter.value = currentEnergy - afterAbilityEnergy;
            SetEnergyBarText ();
        }
        UpdateAbilityUseTime ();
    }

    public float SetTempEnergy (Hand activeHand, float energy)
    {
        activeAbilityEnergyCost[activeHand] = energy;
        float afterAbilityEnergy = GetTotalEnergyUsage ();
        if (afterAbilityEnergy < 0)
        {
            afterAbilityEnergy = 0;
            activeAbilityEnergyCost[activeHand] -= (energy - afterAbilityEnergy);
        }
        energyBarAfter.value = currentEnergy - afterAbilityEnergy;
        SetEnergyBarText ();
        UpdateAbilityUseTime ();
        return energy - afterAbilityEnergy;
    }

    public void DrainRealEnergy (float energy)
    {
        if (currentEnergy > 0)
        {
            currentEnergy -= energy;
            if (currentEnergy < 0)
            {
                currentEnergy = 0;
            }
            energyBarBefore.value = currentEnergy;
            energyBarAfter.value = currentEnergy;
            SetEnergyBarText ();
        }
        UpdateAbilityUseTime ();
    }

    public void UseEnergy (Hand activeHand)
    {
        currentEnergy -= activeAbilityEnergyCost[activeHand];
        energyBarBefore.value = currentEnergy;
        activeAbilityEnergyCost[activeHand] = 0;
    }

    public void CancelEnergyUsage (Hand activeHand)
    {
        energyBarAfter.value = currentEnergy;
        activeAbilityEnergyCost[activeHand] = 0;
        SetEnergyBarText ();
    }

    public void RegenEnergy ()
    {
        if ((Time.time - lastAbilityUsedTime) > regenDelayInSec && currentEnergy < maxEnergy)
        {
            currentEnergy += regenEnergyRate;
            if (currentEnergy > maxEnergy)
            {
                currentEnergy = maxEnergy;
            }
            energyBarBefore.value = currentEnergy;
            energyBarAfter.value = currentEnergy;
            SetEnergyBarText ();
        }
    }

    public bool EnergyIsNotZero ()
    {
        return (currentEnergy - GetTotalEnergyUsage ()) > 0;
    }

    public void SetEnergyBarText ()
    {
        energyBarText.text = (currentEnergy - GetTotalEnergyUsage ()) + " / " + maxEnergy;
    }

    public void UpdateAbilityUseTime ()
    {
        lastAbilityUsedTime = Time.time;
    }

    public void AddHandToActive (Hand activeHand)
    {
        activeAbilityEnergyCost.Add (activeHand, 0);
    }

    public void RemoveHandFromActive (Hand activeHand)
    {
        float entry;
        if (activeAbilityEnergyCost.TryGetValue (activeHand, out entry))
        {
            activeAbilityEnergyCost.Remove (activeHand);
        }
    }

    public float GetRemainingEnergy()
    {
        return currentEnergy - GetTotalEnergyUsage ();
    }

    private float GetTotalEnergyUsage ()
    {
        float totalEnergy = 0;
        foreach (float abilityEnergyCost in activeAbilityEnergyCost.Values)
        {
            totalEnergy += abilityEnergyCost;
        }
        return totalEnergy;
    }

}