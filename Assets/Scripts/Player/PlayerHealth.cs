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
    public bool debugShowHealthText = false;
    private float currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;      
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;
        SetHealthBarText();
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
        else
        {
            // restart game
            GameController.Instance.playerLost();
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

    // restore all health - used when dying & maybe power up 
    public void RecoverAllHealth()
    {
        currentHealth = maxHealth;
        healthBar.value = currentHealth;
        SetHealthBarText();
    }

    public bool HealthIsNotZero()
    {
        return currentHealth > 0;
    }

    public void SetHealthBarText() {
        if(debugShowHealthText)
        {
            healthPointText.text = currentHealth + " / " + maxHealth;
        }
        else
        {
            healthPointText.text = "Health Points";
        }
    }

    public bool HealthIsMax()
    {
        return currentHealth == maxHealth;
    }

}
