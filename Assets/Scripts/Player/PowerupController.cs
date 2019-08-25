using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupController : MonoBehaviour
{

    //[Header ("Powerup Bars")]
    // public GameObject rockClusterBar;
    // public GameObject spikeChainBar;
    // public GameObject earthquakeBar;
    // public GameObject wallPushBar;

    [Header ("Points Required")]
    public int pointsForRockCluster = 1;
    public int pointsForSpikeChain = 1;
    public int pointsForEarthquake = 1;
    public int pointsForWallPush = 1;

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

        if (spikeChainBarCounter >= pointsForSpikeChain)
        {
            PlayerAbility.ToggleSpikeChain ();
            spikeChainBarCounter = 0;
            StartCoroutine (EndSpikeChain ());
        }

        if (earthquakeBarCounter >= pointsForEarthquake)
        {
            // TODO add earthquake functionality
            earthquakeBarCounter = 0;
            StartCoroutine (EndEarthquake ());
        }

        if (wallPushBarCounter >= pointsForWallPush)
        {
            PlayerAbility.ToggleWallPush ();
            wallPushBarCounter = 0;
            StartCoroutine (EndWallPush ());
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