using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergy : MonoBehaviour
{
    public Slider energyBar;
    public Text energyBarText;
    public float maxEnergy;
    public float regenEnergyRate;
    private float currentEnergy;

    // Start is called before the first frame update
    void Start()
    {
        currentEnergy = maxEnergy;
        energyBar.maxValue = maxEnergy;
        energyBar.value = maxEnergy;
        setEnergyBarText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
 
    public void useEnergy(float energy)
    {
        if(currentEnergy > 0) {
            currentEnergy -= energy;
            Debug.Log(currentEnergy);
            energyBar.value = currentEnergy;
            setEnergyBarText();
        }
    }

    public void regenEnergy()
    {
        if(currentEnergy < maxEnergy) {
            currentEnergy += regenEnergyRate;
            if(currentEnergy > maxEnergy) {
                currentEnergy = maxEnergy;
            }
            energyBar.value = currentEnergy;
            setEnergyBarText();
            Debug.Log(currentEnergy);
        }
    }

    public void setEnergyBarText() {
        energyBarText.text = currentEnergy + " / " + maxEnergy;
    }

}
