using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // Play the randomized taiko drum sequence cue
    public void PlayCue()
    {
        StartCoroutine(PlayBeats());
    }

    private IEnumerator PlayBeats()
    {
        for (int i = 0; i < NUM_BEATS; i++)
        {
            audioSource.PlayOneShot(beats[Random.Range(0, beats.Length)]);
            yield return new WaitForSeconds(BEAT_DURATION);
        }
    }
}
