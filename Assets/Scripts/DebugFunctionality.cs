using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugFunctionality : MonoBehaviour
{
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void takeDamage()
    {
        player.GetComponentInChildren<PlayerHealth>().TakeDamage(100);
    }

    public void togglePowerUps()
    {
        PlayerAbility.ToggleRockCluster();
        PlayerAbility.ToggleSpikeChain();
        PlayerAbility.ToggleWallPush();
    }
    
}
