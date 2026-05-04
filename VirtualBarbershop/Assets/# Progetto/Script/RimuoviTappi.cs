using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class RimuoviTappi : MonoBehaviour
{

    AudioSource Audio;
    public GameObject Tappo;
    public AudioClip SuonoApertura;
    public AudioClip SuonoChiusura;

    void Start()
    {
        Audio = this.GetComponent<AudioSource>();
    }



    public void RimuoviTappo()
    { 
         Audio.clip = SuonoApertura;
         Audio.Play();
    }

    public void MettiTappo()
    {
        Audio.clip = SuonoChiusura;
        Audio.Play();
    }
}

