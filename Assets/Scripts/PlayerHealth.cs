using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Slider healthBar;
    public Text healthPointText;
    public float maxHealth = 100;
    public float regenHealthRate;
    private float currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;      
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;
        SetHealthBarText(); 

        TakeDamage(750);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float health)
    {
        if(currentHealth > 0) {
            currentHealth -= health;
            healthBar.value = currentHealth;
            SetHealthBarText();
        }
    }

    public void RegenHealth()
    {
        if(currentHealth < maxHealth) {
            currentHealth += regenHealthRate;
            if(currentHealth > maxHealth) {
                currentHealth = maxHealth;
            }
            healthBar.value = currentHealth;
            SetHealthBarText();
        }
    }

    public bool HealthIsNotZero()
    {
        return currentHealth > 0;
    }

    public void SetHealthBarText() {
        healthPointText.text = currentHealth + " / " + maxHealth;
    }

}
