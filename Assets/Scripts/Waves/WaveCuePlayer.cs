using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]

public class WaveCuePlayer : MonoBehaviour
{
    // The audio source to play the taiko drum wave cues
    public AudioSource audioSource;
    // How many beats to play for the drum cue
    public int NUM_BEATS;
    
    // Drum clips
    [Header("Sounds")] public AudioClip[] beats;

    // How many seconds are in one beat of the drum tracks
    private const float BEAT_DURATION = 60f / 85f;
    
    // True when the drums are playing
    [NonSerialized] public bool playing = false;

    // If taiko drum sequence is not currently playing, play a randomized sequence
    public void PlayCue()
    {
        if (!playing)
        {
            playing = true;
            StartCoroutine(PlayBeats());
        }
    }

    private IEnumerator PlayBeats()
    {
        for (int i = 0; i < NUM_BEATS; i++)
        {
            audioSource.PlayOneShot(beats[Random.Range(0, beats.Length)]);
            yield return new WaitForSeconds(BEAT_DURATION);
        }

        playing = false;
    }
}
