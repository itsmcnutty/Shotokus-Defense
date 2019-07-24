using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnergy : MonoBehaviour
{
    public float maxEnergy;
    public float regenEnergyRate;
    private float currentEnergy;

    // Start is called before the first frame update
    void Start()
    {
        currentEnergy = maxEnergy;       
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
        }
    }

    public void regenEnergy()
    {
        if(currentEnergy < maxEnergy) {
            currentEnergy += regenEnergyRate;
            if(currentEnergy > maxEnergy) {
                currentEnergy = maxEnergy;
            }
            Debug.Log(currentEnergy);
        }
    }

}
