using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugFunctionality : MonoBehaviour
{
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera");
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void takeDamage()
    {
        player.GetComponent<PlayerHealth>().TakeDamage(100);
    }

    public void togglePowerUps()
    {
        PlayerAbility.ToggleRockCluster();
        PlayerAbility.ToggleSpikeChain();
        PlayerAbility.ToggleWallPush();
    }
    
}
