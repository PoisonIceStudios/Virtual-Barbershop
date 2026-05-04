using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Radio : MonoBehaviour
{
    public Transform ManopolaObject;
    public Text UIText;
    public AudioSource[] Stereo;

    public float valueFloat;

    public AudioClip[] Musiche;
    public int numMusic;

    double delay;

    // Start is called before the first frame update
    void Start()
    {

        int random = Random.Range(0, Musiche.Length - 1);
        numMusic = random;

        for (int i = 0; i < Stereo.Length; i++)
        {
            Stereo[i].clip = Musiche[numMusic];
            Stereo[i].Play();
        }

        delay = AudioSettings.dspTime + 0.1;
    }


    // Update is called once per frame
    void Update()
    {

        valueFloat = ManopolaObject.transform.localEulerAngles.y;
        valueFloat = (valueFloat > 180) ? valueFloat - 360 : valueFloat;
        valueFloat = (90.0f + valueFloat) / 180.0f; //-20
        int value = (int)System.Math.Round(valueFloat * 100);

        // Aggiorna volume su tutti gli speaker
        int stereoLen = Stereo.Length;
        for (int i = 0; i < stereoLen; i++)
        {
            Stereo[i].volume = valueFloat - 0.02f;
        }

        if (!Stereo[0].isPlaying)
        {
            numMusic = (numMusic >= Musiche.Length - 1) ? 0 : numMusic + 1;

            delay = AudioSettings.dspTime + 0.1; // aggiorna il campo, non crea variabile locale
            for (int i = 0; i < stereoLen; i++)
            {
                Stereo[i].clip = Musiche[numMusic];
                Stereo[i].PlayScheduled(delay);
            }
        }

        UIText.text = $"volume: {value}%"; // string interpolation, evita allocazioni
    }
}
