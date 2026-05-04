using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BNG
{
    public class Liquido : GrabbableEvents
    {
        public bool LiquidoAttivo;
        public Color ColoreParticelle;
        public ParticleSystem ParticelleLiquido;
        public SnapZone SnapZoneScript;

        public float RotazioneBottigliaX;
        public float RotazioneBottigliaZ;

        public float Quantita = 10f;

        public GameObject ParticelleHitObject;
        public float TimerSpawnHit = 1f;
        float Timer;

        public Transform PuntoGenerazione;

        void Start()
        {
            SnapZoneScript = GetComponentInChildren<SnapZone>();
            var mainModule = ParticelleLiquido.main;
            mainModule.startColor = ColoreParticelle;
        }

        void Update()
        {
            RotazioneBottigliaX = transform.localEulerAngles.x;
            RotazioneBottigliaZ = transform.localEulerAngles.z;

            bool bottiglia_inclinata =
                RotazioneBottigliaZ >= 130 && RotazioneBottigliaZ <= 230 &&
                (RotazioneBottigliaX <= 50 || RotazioneBottigliaX >= 310);

            bool tappoLibero = SnapZoneScript.HeldItem == null;

            if (bottiglia_inclinata && Quantita >= 0 && tappoLibero)
            {
                if (Timer > 0)
                    Timer -= Time.deltaTime;
                else
                {
                    Timer = TimerSpawnHit;
                    Instantiate(ParticelleHitObject, PuntoGenerazione.position, PuntoGenerazione.rotation);
                }

                Quantita -= Time.deltaTime;

                if (!LiquidoAttivo)
                    AttivaLiquido();
            }
            else
            {
                DisattivaLiquido();
            }

            if (!tappoLibero && LiquidoAttivo)
                DisattivaLiquido();
        }

        public void AttivaLiquido()
        {
            if (SnapZoneScript.HeldItem != null) return;

            LiquidoAttivo = true;
            var emission = ParticelleLiquido.emission;
            emission.enabled = true;
        }

        public void DisattivaLiquido()
        {
            LiquidoAttivo = false;
            var emission = ParticelleLiquido.emission;
            emission.enabled = false;
        }

        public void UnparentTappo(GameObject Oggetto)
        {
            Oggetto.transform.parent = null;
        }
    }
}
