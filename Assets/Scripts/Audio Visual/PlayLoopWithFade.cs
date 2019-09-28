﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class PlayLoopWithFade : MonoBehaviour
{
    // The audio source with the loop
    public AudioSource loopingAudioSource;

    [Header("Fading variables")]
    // How long (in seconds) to spend on fade in
    public float fadeInTime;
    // How long (in seconds) to spend on fade out
    public float fadeOutTime;
    
    // How much volume should be modified by each second
    private float volumeUpdatePerSecond = 0;

    // Plays the sound if not already looping
    // Returns false if the sound was already playing
    public bool Play()
    {
        if (!loopingAudioSource.isPlaying)
        {
            // 0 volume and play
            loopingAudioSource.volume = 0;
            loopingAudioSource.Play();
            
            // Calculate how much to update volume per second (regardless of current volume)
            volumeUpdatePerSecond = 1f / fadeInTime;

            return true;
        }
        
        // Sound was already playing
        return false;
    }

    // Stops the sound if it is currently looping (fading in or full volume)
    // Returns false if the sound was not playing when called
    public bool Stop()
    {
        if (loopingAudioSource.isPlaying)
        {
            // Calculate how much to update volume per second (regardless of current volume)
            volumeUpdatePerSecond = -1f / fadeOutTime;
            return true;
        }
        
        // Sound was not playing
        return false;
    }

    // Update is called once per frame. 
    void Update()
    {
        // Update volume if playing
        if (loopingAudioSource.isPlaying)
        {
            loopingAudioSource.volume += volumeUpdatePerSecond * Time.deltaTime;
        }

        if (loopingAudioSource.volume >= 1)
        {
            // Max volume achieved, stop updating volume
            volumeUpdatePerSecond = 0;
        }
        else if (loopingAudioSource.volume <= 0)
        {
            // Min volume achieved, stop updating volume
            loopingAudioSource.Stop();
            volumeUpdatePerSecond = 0;
        }
    }
}
