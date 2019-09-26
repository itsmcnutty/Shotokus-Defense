using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Slider healthBar;
    public Text healthPointText;
    public MeshRenderer damageIndicator;
    public AudioSource audioSource;
    public float maxHealth = 100;
    public float regenHealthRate;
    public bool debugShowHealthText = false;
    public float LOW_HEALTH_THRESHOLD;

    [Header("Sounds")]
    public AudioClip healLoop;
    public AudioClip healFull;
    public AudioSource lowHealth;

    private float prevHealth; // For calculating change in health per frame
    private float currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        Color newColor = new Color(1, 1, 1, 0);
        damageIndicator.material.SetColor("_BaseColor", newColor);
        prevHealth = maxHealth;
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;
        SetHealthBarText();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth > prevHealth)
        {
            // Set pitch based on health (Range 0.5 to 0.8)
            audioSource.pitch = 0.3f * (currentHealth / maxHealth) + 0.5f;
            
            if (!audioSource.loop)
            {
                // If healing and not playing loop, play loop
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else if (audioSource.loop)
        {
            Debug.Log("Stop loop");

            // If not healing and is playing loop, stop playing loop
            audioSource.loop = false;
            audioSource.Stop();
        }

        prevHealth = currentHealth;
    }

    public void TakeDamage(float health)
    {
        if (currentHealth > 0)
        {
            StartCoroutine(FlashDamageIndicator((health / 100) * 255));
            currentHealth -= health;
            healthBar.value = currentHealth;
            SetHealthBarText();
            
            // Start low health loop
            if (currentHealth / maxHealth < LOW_HEALTH_THRESHOLD)
            {
                lowHealth.Play();
            }
        }
        else
        {
            // restart game
            GameController.Instance.playerLost();
        }
    }

    public void RegenHealth()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth += regenHealthRate;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
            healthBar.value = currentHealth;
            SetHealthBarText();
        }
        else
        {
            audioSource.PlayOneShot(healFull);
        }
        
        // Stop low health loop
        if (currentHealth / maxHealth > LOW_HEALTH_THRESHOLD)
        {
            lowHealth.Stop();
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

    public void SetHealthBarText()
    {
        if (debugShowHealthText)
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

    private IEnumerator FlashDamageIndicator(float damagePercent)
    {
        yield return FadeTo(4, 0.05f);
        yield return FadeTo(0, 0.45f);
    }

    private IEnumerator FadeTo(float aValue, float aTime)
    {
        float alpha = damageIndicator.sharedMaterial.color.a;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, aValue, t));
            damageIndicator.sharedMaterial.SetColor("_BaseColor", newColor);
            yield return null;
        }
    }

}