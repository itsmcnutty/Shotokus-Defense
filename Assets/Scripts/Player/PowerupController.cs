using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PowerupController : MonoBehaviour
{

    [Header ("Powerup Bars")]
    public Slider rockClusterBar;
    public Slider spikeChainBar;
    public Slider earthquakeBar;
    public Slider wallPushBar;

    [Header ("Points Required")]
    public int pointsForRockCluster = 10;
    public int pointsForSpikeChain = 10;
    public int pointsForEarthquake = 10;
    public int pointsForWallPush = 10;

    [Header ("Powerup Timer (In Seconds)")]
    public int rockClusterTime = 30;
    public int spikeChainTime = 30;
    public int earthquakeTime = 30;
    public int wallPushTime = 30;

    private static int rockClusterBarCounter = 0;
    private static int spikeChainBarCounter = 0;
    private static int earthquakeBarCounter = 0;
    private static int wallPushBarCounter = 0;
    // Start is called before the first frame update
    void Start ()
    {
        rockClusterBar.maxValue = pointsForRockCluster;
        spikeChainBar.maxValue = pointsForSpikeChain;
        earthquakeBar.maxValue = pointsForEarthquake;
        wallPushBar.maxValue = pointsForWallPush;
    }

    // Update is called once per frame
    void Update ()
    {
        TryActivatePowerUp ();
    }

    private void TryActivatePowerUp ()
    {
        if (rockClusterBarCounter >= pointsForRockCluster)
        {
            PlayerAbility.ToggleRockCluster ();
            rockClusterBarCounter = 0;
            StartCoroutine (EndRockCluster ());
        }
        else if (!PlayerAbility.RockClusterEnabled ())
        {
            rockClusterBar.value = rockClusterBarCounter;
        }

        if (spikeChainBarCounter >= pointsForSpikeChain)
        {
            PlayerAbility.ToggleSpikeChain ();
            spikeChainBarCounter = 0;
            StartCoroutine (EndSpikeChain ());
        }
        else if (!PlayerAbility.SpikeChainEnabled ())
        {
            spikeChainBar.value = spikeChainBarCounter;
        }

        if (earthquakeBarCounter >= pointsForEarthquake)
        {
            PlayerAbility.ToggleEarthquake();
            earthquakeBarCounter = 0;
            StartCoroutine (EndEarthquake ());
        }
        else if(!PlayerAbility.EarthquakeEnabled())
        {
            earthquakeBar.value = earthquakeBarCounter;
        }

        if (wallPushBarCounter >= pointsForWallPush)
        {
            PlayerAbility.ToggleWallPush ();
            wallPushBarCounter = 0;
            StartCoroutine (EndWallPush ());
        }
        else if (!PlayerAbility.WallPushEnabled ())
        {
            wallPushBar.value = wallPushBarCounter;
        }
    }

    private IEnumerator EndRockCluster ()
    {
        float startTime = Time.time;
        while (Time.time - startTime < rockClusterTime)
        {
            yield return DrainAbilityBar (startTime, rockClusterTime, rockClusterBar);
        }
        rockClusterBar.value = 0;
        PlayerAbility.ToggleRockCluster ();
    }

    private IEnumerator EndSpikeChain ()
    {
        float startTime = Time.time;
        while (Time.time - startTime < spikeChainTime)
        {
            yield return DrainAbilityBar (startTime, spikeChainTime, spikeChainBar);
        }
        spikeChainBar.value = 0;
        PlayerAbility.ToggleSpikeChain ();
    }

    private IEnumerator EndEarthquake ()
    {
        float startTime = Time.time;
        while (Time.time - startTime < earthquakeTime)
        {
            yield return DrainAbilityBar (startTime, earthquakeTime, earthquakeBar);
        }
        earthquakeBar.value = 0;
        PlayerAbility.ToggleEarthquake();
    }

    private IEnumerator EndWallPush ()
    {
        float startTime = Time.time;
        while (Time.time - startTime < wallPushTime)
        {
            yield return DrainAbilityBar (startTime, wallPushTime, wallPushBar);
        }
        wallPushBar.value = 0;
        PlayerAbility.ToggleWallPush ();
    }

    private IEnumerator DrainAbilityBar (float startTime, float abilityTime, Slider bar)
    {
        float barValuePercent = (abilityTime - (Time.time - startTime)) / abilityTime;
        bar.value = barValuePercent * bar.maxValue;
        yield return new WaitForEndOfFrame ();
    }

    public static void IncrementRockClusterCounter ()
    {
        rockClusterBarCounter++;
    }

    public static void IncrementSpikeChainCounter ()
    {
        spikeChainBarCounter++;
    }

    public static void IncrementEarthquakeCounter ()
    {
        earthquakeBarCounter++;
    }

    public static void IncrementWallPushCounter ()
    {
        wallPushBarCounter++;
    }
}