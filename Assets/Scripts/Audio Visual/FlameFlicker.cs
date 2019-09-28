using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Random = UnityEngine.Random;

public class FlameFlicker : MonoBehaviour
{
    
    // The point light of this lantern
    public Light pointLight;
    // First color of flame
    public Color color1;
    // Second color of flame
    public Color color2;
    // Min intensity of flame
    public float intensity1;
    // Max intensity of flame
    public float intensity2;
    // How fast the flame flickers
    public float FLICKER_RATE;
    // How close a light must be to update its flame flicker
    public float UPDATE_DIST;
    // How many frames between calculations
    public int CALC_FREQUENCY = 5;
    
    // Main camera game object
    private GameObject mainCamera;
    // Offset to differentiate this noise generation from the others
    private float offset;
    // Square distance is more efficient
    private float sqrUpdateDist;
    // Light position (doesn't change)
    private Vector3 lightPos;
    // Number of update loops
    private long updateCount = 0;

    private void Start()
    {
        // Instantiate stuff
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        offset = Random.Range(0f, 999f);
        sqrUpdateDist = UPDATE_DIST * UPDATE_DIST;
        lightPos = pointLight.transform.position;
        
        // Each light is offset in when they perform their flicker calculations
        updateCount = Random.Range(0, CALC_FREQUENCY);
    }

    // Update is called once per frame
    void Update()
    {
        // Increment count
        updateCount++;
        
        // Only perform calculations every few frames
        if (updateCount % CALC_FREQUENCY == 0)
        {
            Vector3 playerPos = mainCamera.transform.position;

            // Check if light is close enough to update flicker
            if (Vector3.SqrMagnitude(playerPos - lightPos) < sqrUpdateDist)
            {
                // Use Perlin noise algorithm to get 0.0 to 1.0 values
                float lerpValueColor = Mathf.PerlinNoise(Time.time * FLICKER_RATE + offset, 0);
                float lerpValueIntensity = Mathf.PerlinNoise(0, Time.time * FLICKER_RATE + offset);

                // Lerp between colors with this value from noise
                pointLight.color = Color.Lerp(color1, color2, lerpValueColor);

                // Lerp between colors with this value from noise
                pointLight.intensity = Mathf.Lerp(intensity1, intensity2, lerpValueIntensity);
            }
        }
    }
}
