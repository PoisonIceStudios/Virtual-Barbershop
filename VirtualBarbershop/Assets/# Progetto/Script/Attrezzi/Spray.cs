using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BNG
{
    public class Spray : GrabbableEvents
    {
        public bool SprayAttivo;
        public Color ColoreParticelle;
        public ParticleSystem ParticelleSpray;
        public SnapZone SnapZoneScript;

        AudioSource Audio;

        public float Quantita = 10f;

        public GameObject ParticelleHitObject;
        public float TimerSpawnHit = 1f;
        float Timer;

        public Transform PuntoGenerazione;

        void Start()
        {
            Audio = GetComponent<AudioSource>();
            SnapZoneScript = GetComponentInChildren<SnapZone>();

            var mainModule = ParticelleSpray.main;
            mainModule.startColor = ColoreParticelle;
            Timer = TimerSpawnHit;
        }

        void Update()
        {
            if (SprayAttivo)
            {
                if (Timer > 0)
                    Timer -= Time.deltaTime;
                else
                {
                    Timer = TimerSpawnHit;
                    Instantiate(ParticelleHitObject, PuntoGenerazione.position, PuntoGenerazione.rotation);
                }

                if (Quantita <= 0)
                    DisattivaSpray();
                else
                    Quantita -= Time.deltaTime;

#if UNITY_ANDROID
                input.VibrateController(0.4f, 0.4f, 0.1f, thisGrabber.HandSide);
#endif
            }

            if (SnapZoneScript.HeldItem != null && SprayAttivo)
                DisattivaSpray();
        }

        public void AttivaSpray()
        {
            if (SnapZoneScript.HeldItem == null && Quantita >= 0)
            {
                SprayAttivo = true;
                Audio.PlayDelayed(0.05f);

                var emission = ParticelleSpray.emission;
                emission.enabled = true;
            }
        }

        public void DisattivaSpray()
        {
            SprayAttivo = false;
            Audio.Stop();

            if (ParticelleSpray != null)
            {
                var emission = ParticelleSpray.emission;
                emission.enabled = false;
            }
        }

        public void UnparentTappo(GameObject Oggetto)
        {
            Oggetto.transform.parent = null;
        }
    }
}
