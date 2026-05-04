using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;
using UnityEngine.Events;


public class Forbice : GrabbableEvents
{

    public GameObject LamaForbice;
    public float triggerValuePub;
    public Collider[] ColliderTaglio = new Collider[4];
    AudioSource SuonoRottura;
    public AudioSource SuonoTaglio;

    public int ValoreDiGuasto;
    public bool rotto;

    bool cut1 = false;
    bool cut2 = false;
    bool cut3 = false;
    bool cut4 = false;

    public void Start()
    {
        SuonoRottura = this.gameObject.GetComponent<AudioSource>();
        ValoreDiGuasto = Random.Range(1200, 1500);
        if ((rotto == true) && (LamaForbice != null))
        {
            Destroy(LamaForbice);
        }
    }

    public override void OnTrigger(float triggerValue)
    {
        if (rotto == false)
        {

            triggerValuePub = 35 - (triggerValue * 35);
            LamaForbice.transform.localEulerAngles = new Vector3(0, triggerValuePub, 0);

            if ((triggerValuePub < 7) && (cut1 == false)) { ColliderTaglio[3].enabled = true; cut1 = true; }
            else { ColliderTaglio[3].enabled = false; }
            if (triggerValuePub > 7) { cut1 = false; }

            if ((triggerValuePub < 14) && (cut2 == false)) { ColliderTaglio[2].enabled = true; cut2 = true; }
            else { ColliderTaglio[2].enabled = false; }
            if (triggerValuePub > 14) { cut2 = false; }

            if ((triggerValuePub < 21) && (cut3 == false)) { ColliderTaglio[1].enabled = true; cut3 = true; }
            else { ColliderTaglio[1].enabled = false; }
            if (triggerValuePub > 21) { cut3 = false; }

            if ((triggerValuePub < 28) && (cut4 == false)) { ColliderTaglio[0].enabled = true; cut4 = true; SuonoTaglio.Play(); }
            else { ColliderTaglio[0].enabled = false; }
            if (triggerValuePub > 28) { cut4 = false; }

        }

    }

    public override void OnRelease()
    {
        if (rotto == false)
        {
            base.OnRelease();

            for (int i = 0; i < ColliderTaglio.Length; i++)
            {
                ColliderTaglio[i].enabled = false;
            }

            LamaForbice.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
    }


    public void UtilizzaForbice()
    {
        ValoreDiGuasto--;
        if (ValoreDiGuasto <= 0)
        {
            RompiForbice();
        }
    }

    public void RompiForbice()
    {
        rotto = true;

        for (int i = 0; i < ColliderTaglio.Length; i++)
        {
            ColliderTaglio[i].enabled = false;
        }

        SuonoRottura.Play();

        StartCoroutine(RotturaInCorso());

    }

    public IEnumerator RotturaInCorso()
    {
        LamaForbice.AddComponent<Rigidbody>();
        LamaForbice.transform.parent = null;
        yield return new WaitForSeconds(2);
        Destroy(LamaForbice);
    }

}

