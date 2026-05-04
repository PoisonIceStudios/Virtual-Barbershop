using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocaleGenerale : MonoBehaviour
{
    [Header("Soldi Locale : ")]
    [Space(10)]
    public int SoldiTotali;
    public Text TestoCassa;
    public AudioSource SuonoAcquisto;
    public AudioSource SuonoNonAcquisto;

    [Header("Status Locale : ")]
    [Space(10)]
    [Range(0, 100)]
    public int Qualita;
    public GameObject QualitaStelle;
    [Range(0, 100)]
    public int Interni;
    public GameObject InterniStelle;
    [Range(0, 100)]
    public int Progresso;
    public GameObject ProgressoStelle;

    RawImage[] Stelle_1;
    RawImage[] Stelle_2;
    RawImage[] Stelle_3;

    [Space(10)]
    public Text ClientiTotaliAdisor;
    public int ClientiTotali;

    [Space(10)]
    public Text GiorniTotaliAdvisor;
    public int GiorniTotali;

    [Header("Debug : ")]
    [Space(10)]
    public bool AttivaInUpdate = false;


    void Start()
    {

        AttivaInUpdate = false;

        Stelle_1 = QualitaStelle.GetComponentsInChildren<RawImage>();
        Stelle_2 = InterniStelle.GetComponentsInChildren<RawImage>();
        Stelle_3 = ProgressoStelle.GetComponentsInChildren<RawImage>();

      //  UpdateAdvisor();
      //  UpdateCassa();   
    }

    void ControllaStelle()
    {
        for (int i = 0; i < 10; i++)
        {
            if (Qualita > i *10) {
                Stelle_1[i].color = new Color32(255, 215, 0, 255);
            } else {
                Stelle_1[i].color = new Color32(197, 197, 197, 255);
            }

            if (Interni > i * 10)
            {
                Stelle_2[i].color = new Color32(255, 215, 0, 255);
            }
            else
            {
                Stelle_2[i].color = new Color32(197, 197, 197, 255);
            }

            if (Progresso > i * 10) {
                Stelle_3[i].color = new Color32(255, 215, 0, 255);
            } else {
                Stelle_3[i].color = new Color32(197, 197, 197, 255);
            }
        }
    }

    public void UpdateAdvisor()
    {
        ControllaStelle();
        ClientiTotaliAdisor.text = ClientiTotali.ToString();
        GiorniTotaliAdvisor.text = GiorniTotali.ToString();
    }

    public void UpdateCassa()
    {
        TestoCassa.text = SoldiTotali.ToString() + " $";
    }

    public void SuonoAcquistoCassa()
    {
        SuonoAcquisto.Play();
    }

    public void SuonoNonAcquistoCassa()
    {
        SuonoNonAcquisto.Play();
    }

    public void Update()
    {
        if (AttivaInUpdate == true)
        {
            UpdateAdvisor();
            UpdateCassa();
            Debug.Log("Advisor in Update...");
        }
    }
}
