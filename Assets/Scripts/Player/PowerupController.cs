using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PowerupController : MonoBehaviour
{

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

    private static float rockClusterBarCounter = 0;
    private static float spikeChainBarCounter = 0;
    private static float earthquakeBarCounter = 0;
    private static float wallPushBarCounter = 0;

    void Start()
    {   
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
        if (rockClusterBarCounter >= pointsForRockCluster)
        {
            PlayerAbility.ToggleRockCluster();
            rockClusterBarCounter = 0;
            StartCoroutine(EndRockCluster());
        }
        else if (!PlayerAbility.RockClusterEnabled())
        {
            rockClusterBar.fillAmount = rockClusterBarCounter / pointsForRockCluster;
        }

        if (spikeChainBarCounter >= pointsForSpikeChain)
        {
            PlayerAbility.ToggleSpikeChain();
            spikeChainBarCounter = 0;
            StartCoroutine(EndSpikeChain());
        }
        else if (!PlayerAbility.SpikeChainEnabled())
        {
            spikeChainBar.fillAmount = spikeChainBarCounter / pointsForSpikeChain;
        }

        if (earthquakeBarCounter >= pointsForEarthquake)
        {
            PlayerAbility.ToggleEarthquake();
            earthquakeBarCounter = 0;
            StartCoroutine(EndEarthquake());
        }
        else if (!PlayerAbility.EarthquakeEnabled())
        {
            earthquakeBar.fillAmount = earthquakeBarCounter / pointsForEarthquake;
        }

        if (wallPushBarCounter >= pointsForWallPush)
        {
            PlayerAbility.ToggleWallPush();
            wallPushBarCounter = 0;
            StartCoroutine(EndWallPush());
        }
        else if (!PlayerAbility.WallPushEnabled())
        {
            wallPushBar.fillAmount = wallPushBarCounter / pointsForWallPush;
        }
    }

    private IEnumerator EndRockCluster()
    {
        float startTime = Time.time;
        while (Time.time - startTime < rockClusterTime)
        {
            yield return DrainAbilityBar(startTime, rockClusterTime, rockClusterBar);
        }
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
        float barValuePercent = (abilityTime - (Time.time - startTime)) / abilityTime;
        bar.fillAmount = barValuePercent;

        Color baseColor = Color.cyan;
        Color finalColor;
        if (barValuePercent > 0.3f)
        { 
            finalColor = baseColor * Mathf.LinearToGammaSpace(1);
        }
        else
        {
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