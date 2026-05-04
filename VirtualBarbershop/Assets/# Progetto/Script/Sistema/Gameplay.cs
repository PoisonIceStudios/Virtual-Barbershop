using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Gameplay : MonoBehaviour
{

    LocaleGenerale LocaleGeneraleScript;
    BNG.ScreenFader ScreenFaderScript;
    OculusQuestOptimizer OculusQuestOptimizerScript;
    CampanelloPorta CampanelloPorta;
    Campanello CampanelloCliente;
    InGameMenu InGameMenuScript;
    Sottotitoli TutorialScript;
    HairManager HairManagerScript;
    Sottotitoli SottotitoliScript;
    CambiaLocale CambiaLocaleScript;
    ComputerOS ComputerOSScript;

    [Header("Timer : ")]
    public float Tempo = 120;
    public bool AvvioTimer = false;
    [Space(5)]
    public float TempoCliente = 60;
    float tempoClienteS;
    public bool AvvioTimerCliente = false;
    [Space(5)]
    public float TempoTransizioneCliente = 2f;

    [Header("HUD Specchio : ")]
    [Space(10)]
    public GameObject HUDTempo;
    public GameObject HUDTempoTesto;
    [Space(5)]
    public GameObject HUDCampanello;
    public Image HUDCampanelloTimer;

    [Header("Oggetti 3D : ")]
    [Space(10)]
    public GameObject[] Clienti;
    public GameObject Sedia;

    [Header("Finish : ")]
    [Space(10)]
    public Text[] StatusText;
    public int PrezzoAffitto = 50;
    public int TotaleGiorniPerAffitto = 2;


    public int TotaleClientiGiornalieri; // 5 / 7
    float TempoTotaleGiornata = 600f; //10 Minuti
    GameObject ClienteCorrente;
    int ClientiCorrenti;
    bool firstTime;
    bool isTutorial = false;
    public int clienteNumero;

    [HideInInspector]
    public int dayCash; //Soldi Giornata
    [HideInInspector]
    public int SpeseGiornaliere; //Spese Giornaliere
    [HideInInspector]
    public bool VittoriaOneTime; // Vittoria si deve salvare e rimanere salvata 


    private void Start()
        {


        LocaleGeneraleScript = GameObject.Find("# Script").GetComponent<LocaleGenerale>();
        CambiaLocaleScript = GameObject.Find("# Script").GetComponent<CambiaLocale>();
        ScreenFaderScript = GameObject.Find("CenterEyeAnchor").GetComponent<BNG.ScreenFader>();
        OculusQuestOptimizerScript = GameObject.Find("CenterEyeAnchor").GetComponent<OculusQuestOptimizer>();
        CampanelloPorta = GameObject.Find("Statici_3_NoCollider").GetComponent<CampanelloPorta>();
        CampanelloCliente = GameObject.Find("Campanello").GetComponent<Campanello>();
        InGameMenuScript = GameObject.Find("# Script").GetComponent<InGameMenu>();
        SottotitoliScript = GameObject.Find("# Script").GetComponent<Sottotitoli>();
        ComputerOSScript = GameObject.Find("# Script").GetComponent<ComputerOS>();

        if (GameObject.Find("# ScriptTutorial") == null)
        {
            if (ES3.FileExists("Game.es3"))
            {
                ES3AutoSaveMgr.Current.Load();
            }

                //CARICO LA PARTITA SE ESISTE UN SALVATAGGIO DEL GIORNO PRECEDENTE

            if (CambiaLocaleScript.LocaleCorrente != 0)
            {
                StartCoroutine(CambiaLocaleScript.CambioLocaleAvvioCoroutine(CambiaLocaleScript.LocaleCorrente));
            }

            CambiaLocaleScript.ControllaLocaliAcquistati();
            LocaleGeneraleScript.UpdateAdvisor();
            LocaleGeneraleScript.UpdateCassa();

        } else
        {
            TutorialScript = GameObject.Find("# ScriptTutorial").GetComponent<Sottotitoli>();
        }


        TempoCliente = 120;
        if (LocaleGeneraleScript.GiorniTotali <= 30)
        {
            TempoCliente = TempoCliente - (LocaleGeneraleScript.GiorniTotali * 2);
        }
        else
        {
            TempoCliente = 60;
        }


        TotaleClientiGiornalieri = Random.Range(5, 7);

        dayCash = 0;
        SpeseGiornaliere = 0;

        if (LocaleGeneraleScript.Qualita >= 100 && LocaleGeneraleScript.Interni >= 100 && LocaleGeneraleScript.Progresso >= 100 && !VittoriaOneTime) // VITTORIA
        {
            SottotitoliScript.AvviaSottotitoli = true;
            VittoriaOneTime = true;
        } 
        else 
        {
            StartTimer();
        }


       
    }


    public void StartTimer()
    {

        HUDTempo.GetComponent<TextMeshPro>().enabled = true;
        HUDTempoTesto.GetComponent<TextMeshPro>().enabled = true;
        HUDCampanello.GetComponent<TextMeshPro>().enabled = true;
        HUDCampanelloTimer.enabled = false;

        AvvioTimer = true;
    }

    void Update()
    {

        AvvioTimerUpdate(); // DA RIATTIVARE
        TimerCliente();

        if ((Tempo <= 0) && (TempoTotaleGiornata >= 0))
        {
            TempoTotaleGiornata = TempoTotaleGiornata - Time.deltaTime;
        }

    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        HUDTempo.GetComponent<TextMeshPro>().text = string.Format("{0:00}:{1:00}", minutes, seconds);

    }

    public IEnumerator NuovoCliente()
    {

        if (firstTime != false)
        {
            GameObject.FindGameObjectWithTag("Cliente").GetComponent<Animator>().enabled = true;
            yield return new WaitForSeconds(3);
        } 
          else
        {
            firstTime = true;
        }

        ScreenFaderScript.DoFadeIn();
        yield return new WaitForSeconds(TempoTransizioneCliente /2);
        CampanelloPorta.AttivaSuono();

        int ClienteRandom = Random.Range(0, Clienti.Length);

        if (ClienteRandom == clienteNumero)
        {
            ClienteRandom = Random.Range(0, Clienti.Length);
        }

        clienteNumero = ClienteRandom;


        if (ClienteCorrente != null)
        {
            Destroy(ClienteCorrente);
            yield return new WaitForSeconds(TempoTransizioneCliente / 10);
        }

        if (isTutorial != true)
        {

            HUDTempo.GetComponent<TextMeshPro>().enabled = false;
            HUDTempoTesto.GetComponent<TextMeshPro>().enabled = false;
            HUDCampanello.GetComponent<TextMeshPro>().enabled = false;
            yield return new WaitForSeconds(TempoTransizioneCliente / 2);

 
           // Sedia.transform.rotation = new Quaternion(0, -90, Sedia.transform.rotation.z, Sedia.transform.rotation.w);
           ClienteCorrente = Instantiate(Clienti[ClienteRandom], Clienti[ClienteRandom].transform.position, Clienti[ClienteRandom].transform.rotation);
           ClienteCorrente.transform.parent = Sedia.transform;

            //[Header("Static Batching : ")]
           // public GameObject RootCapelli;
           // GameObject[] ArrayStaticBatching = ClienteCorrente.GetComponent<HairManager>().meshObject.ToArray();
           // Debug.Log(ArrayStaticBatching.Length);
           // StaticBatchingUtility.Combine(ArrayStaticBatching, ClienteCorrente.transform.Find("Hair").gameObject);

            ClientiCorrenti++;

            yield return new WaitForSeconds(0.1f);

            OculusQuestOptimizerScript.UpdateOcclusionLayer(100);

            yield return new WaitForSeconds(TempoTransizioneCliente / 3);
            AvvioTimerCliente = true;

            HUDCampanelloTimer.enabled = true;


            tempoClienteS = TempoCliente;

        } else
        {
            HUDCampanelloTimer.enabled = false;
        }

        ScreenFaderScript.DoFadeOut();

        if (GameObject.Find("# ScriptTutorial") != null)
        {
            if (isTutorial == true)
            {
                TutorialScript.ProssimaFrase(3);    
            }
            else
            {
                // Allungo il tempo
                TempoCliente = 300;
                tempoClienteS = TempoCliente;
               
                TutorialScript.ProssimaFrase(4);
                isTutorial = true;
            }
        }
    }

    IEnumerator FinePartita()
    {
        yield return new WaitForSeconds(3);
        ScreenFaderScript.DoFadeIn();

        yield return new WaitForSeconds(TempoTransizioneCliente / 2);
        CampanelloPorta.AttivaSuono();

        if (ClienteCorrente != null)
        {
            Destroy(ClienteCorrente);
        }

        if (ComputerOSScript.PaccoInConsegna == true) // Consegno il pacco se era in consegna
        {
            ComputerOSScript.AnimazioneCorriere.enabled = false;
            ComputerOSScript.SpawnScatola();
        }



        // risultati

        StatusText[0].text = LocaleGeneraleScript.GiorniTotali.ToString();
        StatusText[1].text = ClientiCorrenti.ToString();
        StatusText[2].text = dayCash + " $"; 

        if (LocaleGeneraleScript.GiorniTotali % TotaleGiorniPerAffitto == 0)
        {
            LocaleGeneraleScript.SoldiTotali -= PrezzoAffitto;
            StatusText[3].text = "-" + PrezzoAffitto.ToString() + " $";
        }
        else
        {
            PrezzoAffitto = 0;
            StatusText[3].text = "0 $";
        }

        StatusText[4].text = SpeseGiornaliere + " $"; 
        StatusText[5].text = ((dayCash - PrezzoAffitto) - SpeseGiornaliere).ToString() + " $";

        LocaleGeneraleScript.ClientiTotali = LocaleGeneraleScript.ClientiTotali + ClientiCorrenti;
        LocaleGeneraleScript.GiorniTotali++;

        InGameMenuScript.FinishDay();

        yield return new WaitForSeconds(TempoTransizioneCliente / 3);

        ScreenFaderScript.DoFadeOut();

        LocaleGeneraleScript.UpdateAdvisor();
        LocaleGeneraleScript.UpdateCassa();

    }



    void AvvioTimerUpdate()
    {
        if (AvvioTimer)
        {
            if (Tempo > 0)
            {
                Tempo = Tempo - Time.deltaTime;
                DisplayTime(Tempo);

                if ((CampanelloCliente.StartOneTime == true) && (Tempo >= 10))
                {
                    Tempo = 10;
                    HUDCampanello.GetComponent<TextMeshPro>().enabled = false;
                }
                else if (Tempo <= 10)
                {
                    HUDCampanello.GetComponent<TextMeshPro>().enabled = false;
                }

            }
            else
            {
                Tempo = 0;
                AvvioTimer = false;

                StartCoroutine(NuovoCliente());

            }
        }
    }

    void TimerCliente()
    {

        if (AvvioTimerCliente == true)
        {
            if (TempoCliente > 0)
            {
                if ((CampanelloCliente.StartOneTime != true))
                {
                    TempoCliente -= Time.deltaTime;
                    HUDCampanelloTimer.fillAmount = TempoCliente / tempoClienteS;
                    HUDCampanelloTimer.color = Color.Lerp(Color.red, Color.green, TempoCliente / tempoClienteS);

                } else
                {
                    TempoCliente = 0;
                }
 
            }
            else
            {
                TempoCliente = tempoClienteS + TempoTransizioneCliente;
                AvvioTimerCliente = false;
                CampanelloCliente.AttivaSuono();

                // Attivare Animazione Personaggio e quando finisce cambiare cliente se la giornata non � ancora finita e i clienti non sono finiti

                HairManagerScript = GameObject.FindGameObjectWithTag("Cliente").GetComponent<HairManager>();
                StartCoroutine(HairManagerScript.Confronta(0));



                if ((ClientiCorrenti >= TotaleClientiGiornalieri) || (TempoTotaleGiornata <= 0))
                {
                    //Fine partita
                    StartCoroutine(FinePartita());
                } else
                {
                    //NuovoCliente
                    StartCoroutine(NuovoCliente());
                }
               

            }
        }

        

    }


}