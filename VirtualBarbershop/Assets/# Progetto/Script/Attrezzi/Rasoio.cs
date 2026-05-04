using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rasoio : BNG.GrabbableEvents
{

    Animator m_Animator;

    public GameObject Lama;
    public Collider ColliderLama;
    AudioSource SuonoRottura;
    [HideInInspector]
    public int ValoreDiGuasto;
    [HideInInspector]
    public bool RasoioAttivo = false;
    [HideInInspector]
    public bool rotto;

    void Start()
    {
        SuonoRottura = this.gameObject.GetComponent<AudioSource>();
        ValoreDiGuasto = Random.Range(300, 500);
        m_Animator = GetComponent<Animator>();

        if ((rotto == true) && (Lama != null))
        {
            Destroy(Lama);
        }

    }


    public void RasoioApri()
    {
        m_Animator.SetBool("Status", true);

            ColliderLama.enabled = true;
            RasoioAttivo = true;

    }

    public void RasoioChiudi()
    {
        m_Animator.SetBool("Status", false);

            ColliderLama.enabled = false;
            RasoioAttivo = false;
    }


    public void ApriChiudiRasoio()
    {
        if (rotto == false)
        {
            RasoioAttivo = RasoioAttivo != true;

            if (RasoioAttivo == true)
            {
                RasoioApri();
            }
            else
            {
                RasoioChiudi();
            }
        }
    }


    public void UtilizzoRasoio()
    {
        ValoreDiGuasto--;
        if (ValoreDiGuasto <= 0)
        {
            RompiRasoio();
        }
    }

    public void RompiRasoio()
    {
        rotto = true;
        ColliderLama.enabled = false;
        RasoioAttivo = false;

        SuonoRottura.Play();

        StartCoroutine(RotturaInCorso());

    }

    public IEnumerator RotturaInCorso()
    {
        Lama.AddComponent<Rigidbody>();
        Lama.transform.parent = null;
        yield return new WaitForSeconds(2);
        Destroy(Lama);
    }
}