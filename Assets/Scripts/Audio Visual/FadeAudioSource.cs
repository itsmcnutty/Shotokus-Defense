using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class FadeAudioSource : MonoBehaviour
{
    // The audio source with the loop
    public AudioSource source;

    [Header("Fading variables")]
    // How long (in seconds) to spend on fade in
    public float fadeInTime;
    // How long (in seconds) to spend on fade out
    public float fadeOutTime;

    [Header("Ducking variables")]
    // The audio will duck down if the pitch is not changing
    public bool useDucking;
    // How long the sound takes to duck down
    public float duckTime;
    // How long the sound takes to restore to full volume from ducking
    public float stopDuckTime;
    // How quiet to make the sound when ducking
    public float duckVolume;

    // How much volume should be modified by each second
    private float volumeUpdatePerSecond = 0;
    // True if audio should stop reducing volume at duckVolume
    private bool isCurrentlyDucking = false;

    private void Start()
    {
        // No dividing by 0 allowed
        fadeInTime = fadeInTime <= 0 ? 0.000001f : fadeInTime;
        fadeOutTime = fadeOutTime <= 0 ? 0.000001f : fadeOutTime;
        duckTime = duckTime <= 0 ? 0.000001f : duckTime;
        stopDuckTime = stopDuckTime <= 0 ? 0.000001f : stopDuckTime;
    }

    // Plays the sound if not already looping (Fades back in whether stopped or in middle of fade out)
    // Returns false if the sound was already playing
    public bool Play()
    {
        // Calculate how much to update volume per second (regardless of current volume)
        volumeUpdatePerSecond = 1f / fadeInTime;
        isCurrentlyDucking = false;
        
        if (!source.isPlaying)
        {
            source.Play();
            return true;
        }
        
        // Sound was already playing
        return false;
    }

    // Stops the sound if it is currently looping (fading in or full volume)
    // Returns false if the sound was not playing when called
    public bool Stop()
    {
        if (source.isPlaying)
        {
            // Calculate how much to update volume per second (regardless of current volume)
            volumeUpdatePerSecond = -1f / fadeOutTime;
            isCurrentlyDucking = false;
            return true;
        }
        
        // Sound was not playing
        return false;
    }
    
    // Sets the pitch of the audio source, disabling ducking if it was happening
    public void SetPitch(float newPitch)
    {
        source.pitch = newPitch;
        SetDucking(false);
    }
    
    // Set the volume update rate so that the audio can duck or un-duck as specified
    private void SetDucking(bool duck)
    {
        if (duck)
        {
            isCurrentlyDucking = true;
            volumeUpdatePerSecond = -1f / duckTime;
        }
        else
        {
            isCurrentlyDucking = false;
            volumeUpdatePerSecond = 1f / stopDuckTime;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update volume if playing
        if (source.isPlaying)
        {
            source.volume += volumeUpdatePerSecond * Time.deltaTime;
        }

        // Check if reached volume goal (maximum, muted, ducking volume)
        if (source.volume >= 1)
        {
            // Max volume achieved, stop updating volume and begin ducking if allowed
            volumeUpdatePerSecond = 0;
            if (useDucking)
            {
                SetDucking(true);
            }
        }
        else if (isCurrentlyDucking && source.volume <= duckVolume)
        {
            // Ducking volume achieved, stop updating volume
            source.volume = duckVolume;
            volumeUpdatePerSecond = 0;
        }
        else if (source.volume <= 0)
        {
            // Min volume achieved, stop updating volume and stop playing audio
            source.Stop();
            volumeUpdatePerSecond = 0;
        }
    }
}
