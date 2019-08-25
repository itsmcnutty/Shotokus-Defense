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
        else
        {
            rockClusterBar.value = rockClusterBarCounter;
        }

        if (spikeChainBarCounter >= pointsForSpikeChain)
        {
            PlayerAbility.ToggleSpikeChain ();
            spikeChainBarCounter = 0;
            StartCoroutine (EndSpikeChain ());
        }
        else
        {
            spikeChainBar.value = spikeChainBarCounter;
        }

        if (earthquakeBarCounter >= pointsForEarthquake)
        {
            // TODO add earthquake functionality
            earthquakeBarCounter = 0;
            StartCoroutine (EndEarthquake ());
        }
        else
        {
            earthquakeBar.value = earthquakeBarCounter;
        }

        if (wallPushBarCounter >= pointsForWallPush)
        {
            PlayerAbility.ToggleWallPush ();
            wallPushBarCounter = 0;
            StartCoroutine (EndWallPush ());
        }
        else
        {
            wallPushBar.value = wallPushBarCounter;
        }
    }

    private IEnumerator EndRockCluster ()
    {
        yield return new WaitForSeconds (rockClusterTime);
        PlayerAbility.ToggleRockCluster ();
    }

    private IEnumerator EndSpikeChain ()
    {
        yield return new WaitForSeconds (spikeChainTime);
        PlayerAbility.ToggleSpikeChain ();
    }

    private IEnumerator EndEarthquake ()
    {
        yield return new WaitForSeconds (earthquakeTime);
        // TODO added earthquake functionality
    }

    private IEnumerator EndWallPush ()
    {
        yield return new WaitForSeconds (wallPushTime);
        PlayerAbility.ToggleWallPush ();
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