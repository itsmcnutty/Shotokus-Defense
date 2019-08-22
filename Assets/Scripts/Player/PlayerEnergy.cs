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
        if (infiniteEnergyDisabled)
        {
            float lastEnergyVal;
            if (!activeAbilityEnergyCost.TryGetValue (activeHand, out lastEnergyVal))
            {
                activeAbilityEnergyCost.Add (activeHand, energy);
            }
            else
            {
                activeAbilityEnergyCost[activeHand] = energy;
            }
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
        if (infiniteEnergyDisabled)
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
            }
        }
        UpdateAbilityUseTime ();
    }

    public void UseEnergy (Hand activeHand)
    {
        if (infiniteEnergyDisabled && activeHand != null)
        {
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
        if (infiniteEnergyDisabled)
        {
            float value = RemoveHandFromActive (activeHand);
            SetTempEnergy (newHand, value);
        }
    }

    private void SetEnergyBarText ()
    {
        if(debugShowEnergyText)
        {
            energyBarText.text = Math.Floor (currentEnergy - GetTotalEnergyUsage ()) + " / " + maxEnergy;
        }
        else
        {
            energyBarText.text = "Energy Level";
        }
    }

    private void RegenEnergy ()
    {
        if ((Time.time - lastAbilityUsedTime) > regenDelayInSec && currentEnergy < maxEnergy)
        {
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
        float totalEnergy = 0;
        foreach (float abilityEnergyCost in activeAbilityEnergyCost.Values)
        {
            totalEnergy += abilityEnergyCost;
        }
        return totalEnergy;
    }

}