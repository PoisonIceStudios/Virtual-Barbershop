using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class ComputerOS : MonoBehaviour
{

    LocaleGenerale LocaleGeneraleScript;
    Gameplay GameplayScript;
    Sottotitoli TutorialScript;
    RimuoviScatola RimuoviScatolaScript;

    [Header("Store Settings : ", order = 1)]
    public GameObject PrefabScatola;
    public AudioSource SuonoNonAcquistabile;
    public Vector2 TempoDiSpedizioneMinMax;
    float TempoSpedizione;
    float TempoSpedizioneSaved;
    public Animator AnimazioneCorriere;

    [Header("Hair Store : ", order = 2)]
    [Space(10)]
    public GameObject[] PrefabHairStore;
    public Toggle[] ToggleAcquisto;
    public Text TestoTotaleCassa;
    int TotaleCassa = 0;
    public bool[] ToggleAcquistoInt;

    [Header("Allbay Store : ", order = 3)]
    [Space(10)]
    public GameObject[] PrefabAllbayStore;
    public int[] PrezziSingoliAllBay;

    [Header("Pulsanti : ", order = 4)]
    [Space(10)]
    public Button[] Pulsanti;
    int ValoreIntScelto;

    [Header("Settings : ", order = 5)]
    [Space(10)]
    public InputField inputField;


    [Header("Software & Canvas : ", order = 6)]
    [Space(10)]
    public GameObject SoftwareGameObject;
    public Canvas[] CanvasComputer;
    Collider[] ListaCollider;
    Collider[] ListaColliderSelezionati;

    [Header("Debug : ", order = 7)]
    [Space(10)]
    public bool PaccoInConsegna;
    public bool ArrayDiOggetti;

    [Header("Spawn Settings : ", order = 8)]

    int oggettiTotali = 2;

    private List<GameObject> istanziati = new List<GameObject>();
    private List<Vector3> posizioniOriginali = new List<Vector3>();

    private bool[] premiRiscattati = new bool[3];

    void Start()
    {
        LocaleGeneraleScript = GameObject.Find("# Script").GetComponent<LocaleGenerale>();
        GameplayScript = GameObject.Find("# Script").GetComponent<Gameplay>();
        RimuoviScatolaScript = GameObject.Find("RimuoviScatola").GetComponent<RimuoviScatola>();

        if (GameObject.Find("# ScriptTutorial") != null)
        {
            TutorialScript = GameObject.Find("# ScriptTutorial").GetComponent<Sottotitoli>();
        }


        GameObject ScatolaPreload = Instantiate(PrefabScatola, new Vector3(1200, 1200, 1200), Quaternion.identity);
        Destroy(ScatolaPreload);

        ListaCollider = SoftwareGameObject.GetComponentsInChildren<Collider>();
        foreach (Collider c in ListaCollider) { c.enabled = false; }
    }

    void Update()
    {
        if (PaccoInConsegna == true)
        {
            if (TempoSpedizione <= 0)
            {

                PaccoInConsegna = false;

                AnimazioneCorriere.enabled = true;
                AnimazioneCorriere.Play(0, 0);
                AnimazioneCorriere.Play(0, 1);

                if (TutorialScript != null)
                {
                    TutorialScript.ProssimaFrase(0);
                }

            }
            else
            {
                TempoSpedizione = TempoSpedizione - Time.deltaTime;
            }
        }
    }

    void AcquistoGenerico(int ValoreAcquisto)
    {
        ValoreIntScelto = ValoreAcquisto;
        Pulsanti[ValoreIntScelto].GetComponentInChildren<Text>().text = "SHIPPING...";

        if (TutorialScript != null)
        {
            TempoSpedizione = 10f;
        }
        else
        {
            TempoSpedizione = Random.Range(TempoDiSpedizioneMinMax.x, TempoDiSpedizioneMinMax.y);
            TempoSpedizioneSaved = TempoSpedizione;
        }

        for (int i = 0; i < Pulsanti.Length; i++)
        {
            Pulsanti[i].interactable = false;
        }
        for (int i = 0; i < ToggleAcquisto.Length; i++)
        {
            ToggleAcquisto[i].interactable = false;
        }

        if (ArrayDiOggetti == true)
        {
            LocaleGeneraleScript.SoldiTotali = LocaleGeneraleScript.SoldiTotali - TotaleCassa;
            GameplayScript.SpeseGiornaliere += TotaleCassa;
        }
        else
        {
            LocaleGeneraleScript.SoldiTotali = LocaleGeneraleScript.SoldiTotali - PrezziSingoliAllBay[ValoreIntScelto];
            GameplayScript.SpeseGiornaliere += PrezziSingoliAllBay[ValoreIntScelto];
        }

        LocaleGeneraleScript.UpdateCassa();
        LocaleGeneraleScript.SuonoAcquistoCassa();

        PaccoInConsegna = true;

    }

    public void PulsanteAllBay(int ValoreAcquisto)
    {
        oggettiTotali = 2; // divido il tempo per 2

        if (LocaleGeneraleScript.SoldiTotali >= ValoreAcquisto)
        {
            AcquistoGenerico(ValoreAcquisto);
            StartCoroutine(GeneratoreOggetti(null, PrefabAllbayStore[ValoreAcquisto]));
        }
        else
        {
            LocaleGeneraleScript.SuonoNonAcquistoCassa();
        }
    }

    public void PulsanteHairStore(int ValoreAcquisto)
    {

        oggettiTotali = 1; 

        if ((LocaleGeneraleScript.SoldiTotali >= TotaleCassa) && (TotaleCassa != 0))
        {
            ArrayDiOggetti = true;
            ToggleAcquistoInt = new bool[ToggleAcquisto.Length];

            List<GameObject> listaOggettiDaGenerare = new List<GameObject>();
            for (int i = 0; i < ToggleAcquisto.Length; i++)
            {
                if (ToggleAcquisto[i].isOn)
                {
                    ToggleAcquistoInt[i] = true;
                    ToggleAcquisto[i].isOn = false;
                    listaOggettiDaGenerare.Add(PrefabHairStore[i]);
                    oggettiTotali++;
                }
            }

            AcquistoGenerico(ValoreAcquisto);
            StartCoroutine(GeneratoreOggetti(listaOggettiDaGenerare));

            TotaleCassa = 0;
            TestoTotaleCassa.text = "0 $";

        }
        else
        {
            LocaleGeneraleScript.SuonoNonAcquistoCassa();
        }

        ArrayDiOggetti = false;
        ToggleAcquistoInt = new bool[0];
    }

    public void ClickToggle(int ValoreDollari)
    {
        if (ToggleAcquistoInt.Length == 0)
        {
            if (EventSystem.current.currentSelectedGameObject.GetComponent<Toggle>().isOn == true)
            {
                TotaleCassa = TotaleCassa + ValoreDollari;
            }
            else
            {
                TotaleCassa = TotaleCassa - ValoreDollari;
            }

            TestoTotaleCassa.text = TotaleCassa.ToString() + " $";
        }
    }

    public void ChiudiFinestra(int Finestra)
    {
        CanvasComputer[Finestra].GetComponent<Canvas>().enabled = false;
        foreach (Collider c in ListaCollider) { c.enabled = false; }
    }

    public void ApriFinestra(int Finestra)
    {

        foreach (Canvas bb in CanvasComputer) { bb.enabled = false; }
        CanvasComputer[Finestra].GetComponent<Canvas>().enabled = true;

        foreach (Collider cc in ListaCollider) { cc.enabled = false; }

        ListaColliderSelezionati = CanvasComputer[Finestra].GetComponentsInChildren<Collider>();
        foreach (Collider c in ListaColliderSelezionati) { c.enabled = true; }

    }

    public void SpawnScatola()
    {
        if (RimuoviScatolaScript.Triggers != null)
        {
            RimuoviScatolaScript.RimuoviOggettiScatola();
        }

        StartCoroutine(SpawnScatolaCoroutine());

        foreach (GameObject oggetto in istanziati)
        {
            oggetto.SetActive(true);
        }

        posizioniOriginali.Clear();

        Pulsanti[ValoreIntScelto].GetComponentInChildren<Text>().text = "PURCHASE";

        for (int i = 0; i < Pulsanti.Length; i++)
        {
            Pulsanti[i].interactable = true;
        }
        for (int i = 0; i < ToggleAcquisto.Length; i++)
        {
            ToggleAcquisto[i].interactable = true;
        }

        istanziati.Clear();
    }

    IEnumerator SpawnScatolaCoroutine()
    {
        Instantiate(PrefabScatola, PrefabScatola.transform.position, Quaternion.identity);
        yield return null;
    }

    IEnumerator GeneratoreOggetti(List<GameObject> oggettiDaGenerare = null, GameObject singoloOggetto = null)
    {
        if (oggettiDaGenerare != null)
        {
            foreach (GameObject oggettoPrefab in oggettiDaGenerare)
            {
                yield return new WaitForSeconds(TempoSpedizioneSaved / oggettiTotali);
                GameObject oggetto = Instantiate(oggettoPrefab, new Vector3(1000, 1000, 1000), oggettoPrefab.transform.rotation);
                posizioniOriginali.Add(oggettoPrefab.transform.position);
                istanziati.Add(oggetto);
                oggetto.SetActive(true);
                yield return null;
                oggetto.SetActive(false);
                oggetto.transform.position = posizioniOriginali[istanziati.IndexOf(oggetto)];
            }
        }
        else if (singoloOggetto != null)
        {
            yield return new WaitForSeconds(TempoSpedizioneSaved / oggettiTotali);
            GameObject oggetto = Instantiate(singoloOggetto, new Vector3(1000, 1000, 1000), singoloOggetto.transform.rotation);
            posizioniOriginali.Add(singoloOggetto.transform.position);
            istanziati.Add(oggetto);
            oggetto.SetActive(true);
            yield return null;
            oggetto.SetActive(false);
            oggetto.transform.position = posizioniOriginali[istanziati.IndexOf(oggetto)];

        }
    }


    public void PressButton(string numeroStringa)
    {
        inputField.text = inputField.text + numeroStringa;
    }

    public void getPrize()
    {
        switch (inputField.text)
        {

            // se aggiungo codici devo ricordarmi di incrementare i booleani "premiRiscattati"
            case "058392": // Ricompensa 250$

                if (premiRiscattati[0] != true)
                {
                    LocaleGeneraleScript.SoldiTotali = LocaleGeneraleScript.SoldiTotali + 250;
                    LocaleGeneraleScript.UpdateCassa();
                    LocaleGeneraleScript.SuonoAcquistoCassa();
                    premiRiscattati[0] = true;
                    ChiudiFinestra(4);
                }
                else
                {
                    LocaleGeneraleScript.SuonoNonAcquistoCassa();
                }
                break;

            case "736113": // Ricompensa 500$
                if (premiRiscattati[1] != true)
                {
                    LocaleGeneraleScript.SoldiTotali = LocaleGeneraleScript.SoldiTotali + 500;
                    LocaleGeneraleScript.UpdateCassa();
                    LocaleGeneraleScript.SuonoAcquistoCassa();
                    premiRiscattati[1] = true;
                    ChiudiFinestra(4);
                }
                else
                {
                    LocaleGeneraleScript.SuonoNonAcquistoCassa();
                }
                break;

            case "483982": // Ricompensa 2000$
                if (premiRiscattati[2] != true)
                {
                    LocaleGeneraleScript.SoldiTotali = LocaleGeneraleScript.SoldiTotali + 2000;
                    LocaleGeneraleScript.UpdateCassa();
                    LocaleGeneraleScript.SuonoAcquistoCassa();
                    premiRiscattati[2] = true;
                    ChiudiFinestra(4);
                } else
                {
                    LocaleGeneraleScript.SuonoNonAcquistoCassa();
                }
                break;

            default:
                LocaleGeneraleScript.SuonoNonAcquistoCassa();
                break;
        }
        inputField.text = "";
    }

}

