using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectDestructor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //
        AudioSource audioSource = GetComponent<AudioSource>();
        float lifeTime = audioSource.clip.length;
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
