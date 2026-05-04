using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampanelloPorta : MonoBehaviour
{
    AudioSource audioSource;

    void Start()
    {
        audioSource = this.GetComponent<AudioSource>(); 
    }

    void OnTriggerEnter()
    {
        audioSource.Play();
    }

    public void AttivaSuono()
    {
        audioSource.Play();
    }

}
