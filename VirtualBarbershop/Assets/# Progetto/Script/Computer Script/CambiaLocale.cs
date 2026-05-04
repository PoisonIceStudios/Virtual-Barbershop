using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CambiaLocale : MonoBehaviour
{

    LocaleGenerale LocaleScript;
    Gameplay GameplayScript;
    BNG.ScreenFader ScreenFaderScript;
    int StileNumeroGen;

    [Header("Info : ")]
    [Space(10)]
    public int LocaleCorrente;
    public bool[] LocaleAcquistato = new bool[4]; //4 locali totali

    [Header("Pulsanti Stile Locale : ")]
    [Space(10)]
    public Button[] Pulsanti;
    public int[] Prezzi;
    public int[] PuntiBellezza;
    public int BellezzaCorrente;

    [Header("Oggetti : ")]
    [Space(10)]
    public GameObject Dinamici;
    public GameObject DinamiciPlayerCollider;
    MeshRenderer[] DinamiciChild;
    MeshRenderer[] DinamiciPlayerColliderChild;
    public GameObject Statici1;
    public GameObject Statici2;
    public GameObject Statici3;
    public GameObject Statici3_NoCollider;
    public GameObject Locale1;
    public GameObject Locale2;
    public GameObject Locale3_Tetto;
    public GameObject Locale3_Pavimento;
    public GameObject Locale4;

    [Header("Materiali : ")]
    [Space(10)]
    public Material[] MaterialiDinamici;
    public Material[] MaterialiStatici1;
    public Material[] MaterialiStatici2;
    public Material[] MaterialiStatici3;
    public Material[] MaterialiLocale1;
    public Material[] MaterialiLocale2;
    public Material[] MaterialiLocale3;
    public Material[] MaterialiPavimentoTetto;

    [Header("Pulsanti Stile Locale : ")]
    [Space(10)]
    public AudioSource AudioCambioLocale;

    [Header("Transizione : ")]
    [Space(10)]
    public float TimerFade = 1f;

    [Header("Debug : ")]
    [Space(10)]
    public bool CambiaLocaleDebug;
    [Range(0,3)]
    public int Numero;

    void Start()
    {
        LocaleScript = this.GetComponent<LocaleGenerale>();
        GameplayScript = GameObject.Find("# Script").GetComponent<Gameplay>();

        ScreenFaderScript = GameObject.Find("CenterEyeAnchor").GetComponent<BNG.ScreenFader>();
        ScreenFaderScript.FadeInSpeed = TimerFade;
        ScreenFaderScript.FadeOutSpeed = TimerFade;
        ScreenFaderScript.FadeColor = new Color32(0, 0, 0, 255);

        CambiaBellezzaInterni();

        DinamiciChild = Dinamici.GetComponentsInChildren<MeshRenderer>();
        DinamiciPlayerColliderChild = DinamiciPlayerCollider.GetComponentsInChildren<MeshRenderer>();
 
    }


    public void CambiaStileLocale(int StileNumero)
    {
        StileNumeroGen = StileNumero;
        var testopulsante = Pulsanti[StileNumeroGen].GetComponentInChildren<Text>().text;

        if (testopulsante == "PURCHASE")
        {
            if (Prezzi[StileNumeroGen] <= LocaleScript.SoldiTotali)
            {
                // CAMBIO LOCALE
                Pulsanti[StileNumeroGen].GetComponentInChildren<Text>().text = "SELECT";
                LocaleAcquistato[StileNumeroGen] = true;

                LocaleScript.SoldiTotali = LocaleScript.SoldiTotali - Prezzi[StileNumeroGen];
                LocaleScript.UpdateCassa();
                LocaleScript.SuonoAcquistoCassa();
                GameplayScript.SpeseGiornaliere += Prezzi[StileNumeroGen];
            } else
            {
                // NO SOLDI
                LocaleScript.SuonoNonAcquistoCassa();
            }
        }
        if (testopulsante == "SELECT")
        {
            StartCoroutine("CambiaMaterialiLocale");
        }
    }

    public void ControllaLocaliAcquistati()
    {
        for (int i = 0; i < LocaleAcquistato.Length; i++)
        {
            if (LocaleAcquistato[i] == true)
            {
                if (i == LocaleCorrente)
                {
                    Pulsanti[i].GetComponentInChildren<Text>().text = "IN USE";
                } 
                else
                {
                    Pulsanti[i].GetComponentInChildren<Text>().text = "SELECT";
                }
            }
        }
    }

    public void CambiaBellezzaInterni()
    {
        for (int i = 0; i < Pulsanti.Length; i++)
        {
            if (Pulsanti[i].GetComponentInChildren<Text>().text == "IN USE")
            {
                LocaleScript.Interni = LocaleScript.Interni - BellezzaCorrente;
                LocaleScript.Interni = LocaleScript.Interni + PuntiBellezza[i];
                BellezzaCorrente = PuntiBellezza[i];
            }
        }
        // LocaleScript.UpdateAdvisor();  forse è meglio metterlo nel giorno successivo e non in realtime
    }

    public IEnumerator CambiaMaterialiLocale()
    {
        for (int i = 0; i < Pulsanti.Length; i++)
        {
            if (i != StileNumeroGen)
            {
                if (Pulsanti[i].GetComponentInChildren<Text>().text != "PURCHASE")
                {
                    Pulsanti[i].GetComponentInChildren<Text>().text = "SELECT";
                }
            }
            else
            {
                Pulsanti[i].GetComponentInChildren<Text>().text = "IN USE";

                // Cambia Locale
                for (int x = 0; x < Pulsanti.Length; x++)
                {
                    Pulsanti[x].interactable = false;
                }

                ScreenFaderScript.DoFadeIn();

                yield return new WaitForSeconds(1.5f);

                AudioCambioLocale.Play();

                for (int e = 0; e < DinamiciChild.Length; e++)
                {
                    if (DinamiciChild[e].tag != "CestinoUI")
                    {
                        yield return new WaitForSeconds(0.06f);
                        DinamiciChild[e].material = MaterialiDinamici[i];
                    }
                }
                for (int e = 0; e < DinamiciPlayerColliderChild.Length - 1; e++) //meno il vetro
                {
                    if (DinamiciChild[e].tag != "CestinoUI")
                    {
                        yield return new WaitForSeconds(0.06f);
                        DinamiciPlayerColliderChild[e].material = MaterialiDinamici[i];
                    }
                }

                yield return new WaitForSeconds(0.06f);
                Statici1.GetComponent<MeshRenderer>().material = MaterialiStatici1[i];
                yield return new WaitForSeconds(0.06f);
                Statici2.GetComponent<MeshRenderer>().material = MaterialiStatici2[i];
                yield return new WaitForSeconds(0.06f);
                Statici3.GetComponent<MeshRenderer>().material = MaterialiStatici3[i];
                yield return new WaitForSeconds(0.06f);
                Statici3_NoCollider.GetComponent<MeshRenderer>().material = MaterialiStatici3[i];
                yield return new WaitForSeconds(0.06f);
                Locale1.GetComponent<MeshRenderer>().material = MaterialiLocale1[i];
                yield return new WaitForSeconds(0.06f);
                Locale2.GetComponent<MeshRenderer>().material = MaterialiLocale2[i];
                yield return new WaitForSeconds(0.06f);
                Locale3_Tetto.GetComponent<MeshRenderer>().material = MaterialiPavimentoTetto[i];
                yield return new WaitForSeconds(0.06f);
                Locale3_Pavimento.GetComponent<MeshRenderer>().material = MaterialiPavimentoTetto[i];
                yield return new WaitForSeconds(0.06f);
                Locale4.GetComponent<MeshRenderer>().material = MaterialiLocale3[i];

                for (int x = 0; x < Pulsanti.Length; x++)
                {
                    Pulsanti[x].interactable = true;
                }

                yield return new WaitForSeconds(2.5f);
                LocaleCorrente = i;

                ScreenFaderScript.DoFadeOut();
            }
        }

        CambiaBellezzaInterni();
        yield return null;
    }

    public IEnumerator CambioLocaleAvvioCoroutine( int NumeroLocale)
    {
        for (int e = 0; e < DinamiciChild.Length; e++)
        {
            if (DinamiciChild[e].tag != "CestinoUI")
            {
                DinamiciChild[e].material = MaterialiDinamici[NumeroLocale];
            }
        }
        for (int e = 0; e < DinamiciPlayerColliderChild.Length - 1; e++) //meno il vetro
        {
            if (DinamiciChild[e].tag != "CestinoUI")
            {

                DinamiciPlayerColliderChild[e].material = MaterialiDinamici[NumeroLocale];
            }
        }

        Statici1.GetComponent<MeshRenderer>().material = MaterialiStatici1[NumeroLocale];
        Statici2.GetComponent<MeshRenderer>().material = MaterialiStatici2[NumeroLocale];
        Statici3.GetComponent<MeshRenderer>().material = MaterialiStatici3[NumeroLocale];
        Statici3_NoCollider.GetComponent<MeshRenderer>().material = MaterialiStatici3[NumeroLocale];
        Locale1.GetComponent<MeshRenderer>().material = MaterialiLocale1[NumeroLocale];
        Locale2.GetComponent<MeshRenderer>().material = MaterialiLocale2[NumeroLocale];
        Locale3_Tetto.GetComponent<MeshRenderer>().material = MaterialiPavimentoTetto[NumeroLocale];
        Locale3_Pavimento.GetComponent<MeshRenderer>().material = MaterialiPavimentoTetto[NumeroLocale];
        Locale4.GetComponent<MeshRenderer>().material = MaterialiLocale3[NumeroLocale];

        LocaleCorrente = NumeroLocale;

        yield return null;

    }

    private void Update()
    {
        if (CambiaLocaleDebug)
        {
            CambiaLocaleDebug = false;
            StartCoroutine(CambioLocaleAvvioCoroutine(Numero));
        }
    }
}
