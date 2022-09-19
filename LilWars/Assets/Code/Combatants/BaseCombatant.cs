using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCombatant : MonoBehaviour
{

    private AudioSource audioSource;

    // De momento nada, luego ya podnremos cosas en común


    protected virtual void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    protected void PlayClipWithoutOverlapping(AudioClip clip, float volume = 0.5f)
    {
        //
        if (audioSource.clip == clip && audioSource.isPlaying)
            return;
        //
        audioSource.volume = volume;
        audioSource.clip = clip;
        audioSource.Play();
    }
}
