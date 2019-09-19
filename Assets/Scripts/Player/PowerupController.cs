using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PowerupController : MonoBehaviour
{
    [Header("Powerup Canvas")]
    public GameObject rockClusterCanvas;
    public GameObject spikeChainCanvas;
    public GameObject earthquakeCanvas;
    public GameObject wallPushCanvas;

    [Header("Powerup Bars")]
    public Image rockClusterBar;
    public Image spikeChainBar;
    public Image earthquakeBar;
    public Image wallPushBar;

    [Header("Points Required")]
    public float pointsForRockCluster = 10;
    public float pointsForSpikeChain = 10;
    public float pointsForEarthquake = 10;
    public float pointsForWallPush = 10;

    [Header("Powerup Timer (In Seconds)")]
    public int rockClusterTime = 30;
    public int spikeChainTime = 30;
    public int earthquakeTime = 30;
    public int wallPushTime = 30;

    public static PowerupController instance;

    private static float rockClusterBarCounter = 0;
    private static float spikeChainBarCounter = 0;
    private static float earthquakeBarCounter = 0;
    private static float wallPushBarCounter = 0;

    // Instance getter and initialization
    public static PowerupController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType(typeof(PowerupController)) as PowerupController;
            }
            return instance;
        }
    }

    void Start()
    {   
        // Resets the color of the powerup material
        rockClusterBar.material.SetColor("_EmissionColor", Color.white);
        spikeChainBar.material.SetColor("_EmissionColor", Color.white);
        earthquakeBar.material.SetColor("_EmissionColor", Color.white);
        wallPushBar.material.SetColor("_EmissionColor", Color.white);
    }

    // Update is called once per frame
    void Update()
    {
        TryActivatePowerUp();
    }

    private void TryActivatePowerUp()
    {
        // For each of the abilities:
        //      Checks that the counter has surpassed the points needed to activate
        //      Toggle the ability to be active
        //      Starts a Co-Routine to activate the ability
        //      Reset ability counter
        if (rockClusterBarCounter >= pointsForRockCluster)
        {
            PlayerAbility.ToggleRockCluster();
            rockClusterBarCounter = 0;
            StartCoroutine(EndRockCluster());
        }
        else if (!PlayerAbility.RockClusterEnabled)
        {
            rockClusterBar.fillAmount = rockClusterBarCounter / pointsForRockCluster;
        }

        if (spikeChainBarCounter >= pointsForSpikeChain)
        {
            PlayerAbility.ToggleSpikeChain();
            spikeChainBarCounter = 0;
            StartCoroutine(EndSpikeChain());
        }
        else if (!PlayerAbility.SpikeChainEnabled)
        {
            spikeChainBar.fillAmount = spikeChainBarCounter / pointsForSpikeChain;
        }

        if (earthquakeBarCounter >= pointsForEarthquake)
        {
            PlayerAbility.ToggleEarthquake();
            earthquakeBarCounter = 0;
            StartCoroutine(EndEarthquake());
        }
        else if (!PlayerAbility.EarthquakeEnabled)
        {
            earthquakeBar.fillAmount = earthquakeBarCounter / pointsForEarthquake;
        }

        if (wallPushBarCounter >= pointsForWallPush)
        {
            PlayerAbility.ToggleWallPush();
            wallPushBarCounter = 0;
            StartCoroutine(EndWallPush());
        }
        else if (!PlayerAbility.WallPushEnabled)
        {
            wallPushBar.fillAmount = wallPushBarCounter / pointsForWallPush;
        }
    }

    private IEnumerator EndRockCluster()
    {
        // The power-up is active for the alloted amount of time
        float startTime = Time.time;
        while (Time.time - startTime < rockClusterTime)
        {
            yield return DrainAbilityBar(startTime, rockClusterTime, rockClusterBar);
        }

        // Resets power-up when time runs out
        rockClusterBar.fillAmount = 0;
        rockClusterBar.material.SetColor("_EmissionColor", Color.white);
        PlayerAbility.ToggleRockCluster();
    }

    private IEnumerator EndSpikeChain()
    {
        float startTime = Time.time;
        while (Time.time - startTime < spikeChainTime)
        {
            yield return DrainAbilityBar(startTime, spikeChainTime, spikeChainBar);
        }
        spikeChainBar.fillAmount = 0;
        spikeChainBar.material.SetColor("_EmissionColor", Color.white);
        PlayerAbility.ToggleSpikeChain();
    }

    private IEnumerator EndEarthquake()
    {
        float startTime = Time.time;
        while (Time.time - startTime < earthquakeTime)
        {
            yield return DrainAbilityBar(startTime, earthquakeTime, earthquakeBar);
        }
        earthquakeBar.fillAmount = 0;
        earthquakeBar.material.SetColor("_EmissionColor", Color.white);
        PlayerAbility.ToggleEarthquake();
    }

    private IEnumerator EndWallPush()
    {
        float startTime = Time.time;
        while (Time.time - startTime < wallPushTime)
        {
            yield return DrainAbilityBar(startTime, wallPushTime, wallPushBar);
        }
        wallPushBar.fillAmount = 0;
        wallPushBar.material.SetColor("_EmissionColor", Color.white);
        PlayerAbility.ToggleWallPush();
    }

    private IEnumerator DrainAbilityBar(float startTime, float abilityTime, Image bar)
    {
        // Gets the current status of the power-up drain
        float barValuePercent = (abilityTime - (Time.time - startTime)) / abilityTime;
        bar.fillAmount = barValuePercent;

        Color baseColor = Color.cyan;
        Color finalColor;
        if (barValuePercent > 0.3f)
        { 
            // Power-up gauge set to a normal color above 30% time remaining
            finalColor = baseColor * Mathf.LinearToGammaSpace(1);
        }
        else
        {
            // Power-up starts to blink under 30% time remaining
            float emission = Mathf.PingPong(Time.time, 1.0f);
            finalColor = baseColor * Mathf.LinearToGammaSpace(emission);
        }
        bar.material.SetColor("_EmissionColor", finalColor);

        yield return new WaitForEndOfFrame();
    }

    public static void IncrementRockClusterCounter()
    {
        rockClusterBarCounter++;
    }

    public static void IncrementSpikeChainCounter()
    {
        spikeChainBarCounter++;
    }

    public static void IncrementEarthquakeCounter()
    {
        earthquakeBarCounter++;
    }

    public static void IncrementWallPushCounter()
    {
        wallPushBarCounter++;
    }
}