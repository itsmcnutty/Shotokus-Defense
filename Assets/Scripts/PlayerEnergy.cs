using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergy : MonoBehaviour
{
    public enum AbilityType
    {
        Heal,
        Rock,
        Spike,
        Wall,
        Quicksand
    }

    public Slider energyBarBefore;
    public Slider energyBarAfter;
    public Text energyBarText;
    public float maxEnergy;
    public float regenEnergyRate;
    public float regenDelayInSec;
    private float currentEnergy;
    private float afterAbilityEnergy;
    private float lastAbilityUsedTime;
    private List<AbilityType> activeAbilities;

    // Start is called before the first frame update
    void Start ()
    {
        activeAbilities = new List<AbilityType>();
        currentEnergy = maxEnergy;
        afterAbilityEnergy = maxEnergy;
        energyBarBefore.maxValue = maxEnergy;
        energyBarBefore.value = maxEnergy;
        energyBarAfter.maxValue = maxEnergy;
        energyBarAfter.value = maxEnergy;
        SetEnergyBarText ();
    }

    // Update is called once per frame
    void Update ()
    {
        Debug.Log(activeAbilities.Count);
    }

    public void DrainTempEnergy (float energy)
    {
        if (afterAbilityEnergy > 0)
        {
            afterAbilityEnergy -= energy;
            if (afterAbilityEnergy < 0)
            {
                afterAbilityEnergy = 0;
            }
            energyBarAfter.value = afterAbilityEnergy;
            SetEnergyBarText ();
        }
        UpdateAbilityUseTime ();
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
            afterAbilityEnergy = currentEnergy;
            energyBarBefore.value = currentEnergy;
            energyBarAfter.value = currentEnergy;
            SetEnergyBarText ();
        }
        UpdateAbilityUseTime ();
    }

    public void UseEnergy (AbilityType type)
    {
        currentEnergy = afterAbilityEnergy;
        energyBarBefore.value = currentEnergy;
        RemoveActiveAbility (type);
    }

    public void CancelEnergyUsage (AbilityType type)
    {
        afterAbilityEnergy = currentEnergy;
        energyBarAfter.value = currentEnergy;
        SetEnergyBarText ();
        RemoveActiveAbility (type);
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
            afterAbilityEnergy = currentEnergy;
            energyBarBefore.value = currentEnergy;
            energyBarAfter.value = currentEnergy;
            SetEnergyBarText ();
        }
    }

    public bool EnergyIsNotZero ()
    {
        return afterAbilityEnergy > 0;
    }

    public void SetEnergyBarText ()
    {
        energyBarText.text = afterAbilityEnergy + " / " + maxEnergy;
    }

    public void UpdateAbilityUseTime ()
    {
        lastAbilityUsedTime = Time.time;
    }

    public bool AbilityIsActive (AbilityType type)
    {
        return activeAbilities.Contains (type);
    }

    public bool HealAbilityIsActive ()
    {
        return activeAbilities.Contains (AbilityType.Heal);
    }

    public bool RockAbilityIsActive ()
    {
        return activeAbilities.Contains (AbilityType.Rock) ||
            activeAbilities.Contains (AbilityType.Spike) ||
            activeAbilities.Contains (AbilityType.Wall) ||
            activeAbilities.Contains (AbilityType.Quicksand);
    }

    public void AddActiveAbility (AbilityType type)
    {
        activeAbilities.Add (type);
    }

    public void RemoveActiveAbility (AbilityType type)
    {
        if (activeAbilities.Contains (type))
        {
            activeAbilities.Remove (type);
        }
    }

}