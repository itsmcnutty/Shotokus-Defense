using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioMultiClipSource : MonoBehaviour
{
    // The audio source with the loop
    public AudioSource source;
    // The multiple clips to be played by this audio source
    public AudioClip[] clips;
    // The random pitch shift to add when playing sounds
    public float minPitch;
    public float maxPitch;
    // How long the source should block calls to Play() after playing once
    public float delayPlayTime;
    
    // Game time of the last time a sound was played from this source
    private float lastTime = 0;

    // Plays a random sound from the clips array
    public void PlayRandom()
    {
        source.clip = clips[Random.Range(0, clips.Length)];
        PlayWithRandomPitch();
    }
    
    // Maps the given parameter from [0.0 to 1.0] to [0, clips.length) and plays the sound at the corresponding
    // position in the array
    public void PlayFromParam(float param)
    {
        int numClips = clips.Length;
        
        // Clamp to [0.0, 1.0] and scale to number of clips
        param = param > 1f ? 1f : (param < 0f ? 0f : param);
        int clipIndex = (int) (param * numClips);
        
        // If param was 1.0, subtract 1 to keep index in bounds
        if (clipIndex == numClips)
        {
            clipIndex = clipIndex - 1;
        }
        
        // Play selected audio
        source.clip = clips[clipIndex];
        PlayWithRandomPitch();
    }

    // Sets a random pitch for the source between the min and max pitch properties and plays the sound
    private void PlayWithRandomPitch()
    {
        // Block playing sound if played within the last delayPlayTime seconds
        if (Time.time - lastTime < delayPlayTime)
        {
            return;
        }
        
        source.pitch = Random.Range(minPitch, maxPitch);
        source.Play();
        lastTime = Time.time;
    }
}
