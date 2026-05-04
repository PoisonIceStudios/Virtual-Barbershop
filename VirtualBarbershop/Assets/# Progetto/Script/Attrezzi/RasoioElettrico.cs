using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;


    public class RasoioElettrico : GrabbableEvents {

        Grabbable grabbable;

        public AudioClip OnClick;
        public AudioClip OffClick;
        public Collider ColliderLama;
        public float TimerDiGuasto;
        public GameObject OggettoGuasto;

        AudioSource Audio;
        Animator m_Animator;

        bool RasoioAttivo = false;
        [HideInInspector]
        public bool Guasto;


    public void Update()
        {
       

        if (RasoioAttivo == true && Guasto == false) { 
            if (TimerDiGuasto <= 0)
            {
                Guasto = true;
                OggettoGuasto.GetComponent<AudioSource>().Play();
                OggettoGuasto.GetComponent<ParticleSystem>().Play();
                ColliderLama.enabled = false;
                Audio.Stop();
                RasoioAttivo = false;
                m_Animator.SetBool("Guasto", true);

            }
            else
            {
                TimerDiGuasto = TimerDiGuasto - Time.deltaTime;

                #if UNITY_ANDROID
                input.VibrateController(0.4f, 0.4f, 0.1f, thisGrabber.HandSide);
                #endif
            }
        }

        }

        public void Start()
        {
            grabbable = GetComponent<Grabbable>();
            Audio = GetComponent<AudioSource>();
            m_Animator = GetComponent<Animator>();
        }

        public void RasoioOn()
        {
            m_Animator.SetBool("Status", true);
            Audio.PlayOneShot(OnClick);

            if (Guasto == false)
            {
                ColliderLama.enabled = true;
                Audio.PlayDelayed(0.05f);
                RasoioAttivo = true;
            }
        }

        public void RasoioOff()
        {
            m_Animator.SetBool("Status", false);
            Audio.PlayOneShot(OffClick);

            if (Guasto == false)
            {
                ColliderLama.enabled = false;
                Audio.Stop();    
                RasoioAttivo = false;
            }
        }


        public void AttivaDisattivaRasoio()
        {
            RasoioAttivo = RasoioAttivo != true;

            if (RasoioAttivo == true)
            {
                RasoioOn();
            } else
            {
                RasoioOff();
            }
        }

        public void HideHighlightTutorial()
        {
          this.gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        }
    }

