using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergy : MonoBehaviour
{
    public enum AbilityType {
        None,
        Heal,
        Rock,
        Spike,
        Wall,
        Quicksand
    };

    public Slider energyBar;
    public Text energyBarText;
    public float maxEnergy;
    public float regenEnergyRate;
    public float regenDelayInSec;
    private float currentEnergy;
    private float lastAbilityUsedTime;
    private AbilityType activeAbility;

    // Start is called before the first frame update
    void Start()
    {
        currentEnergy = maxEnergy;
        energyBar.maxValue = maxEnergy;
        energyBar.value = maxEnergy;
        SetEnergyBarText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
 
    public void UseEnergy(float energy, AbilityType type)
    {
        SetActiveAbility(type);
        if(currentEnergy > 0) {
            currentEnergy -= energy;
            if(currentEnergy < 0) {
                currentEnergy = 0;
            }
            energyBar.value = currentEnergy;
            SetEnergyBarText();
        }
        UpdateAbilityUseTime();
    }

    public void RegenEnergy()
    {
        if((Time.time - lastAbilityUsedTime) > regenDelayInSec && currentEnergy < maxEnergy) {
            currentEnergy += regenEnergyRate;
            if(currentEnergy > maxEnergy) {
                currentEnergy = maxEnergy;
            }
            energyBar.value = currentEnergy;
            SetEnergyBarText();
        }
    }

    public bool EnergyIsNotZero()
    {
        return currentEnergy > 0;
    }

    public void SetEnergyBarText()
    {
        energyBarText.text = currentEnergy + " / " + maxEnergy;
    }

    public void UpdateAbilityUseTime()
    {
        lastAbilityUsedTime = Time.time;
    }

    public bool AbilityIsActive(AbilityType type)
    {
        return type == activeAbility;
    }

    public void SetActiveAbility(AbilityType type)
    {
        activeAbility = type;
    }

}
