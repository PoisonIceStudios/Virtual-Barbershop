using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using UnityEngine.SceneManagement;
using TMPro;


public class Sottotitoli : MonoBehaviour
{

    Gameplay GameplayScript;
    LocaleGenerale LocaleGeneraleScript;
    BNG.ScreenFader ScreenFaderScript;

    public float timerInizio = 5f;
    bool timerbool = false;

    AudioSource Audio;
    bool InPlay;

    [HideInInspector]
    public int SubNumber = 0;

    int NumeroFrasiPub;

    public AudioClip[] AudioFiles;
    public string[] SottotitoliTesti;
    public string[] OggettiEventi;
    public GameObject[] OggettiInstance;
    public GameObject[] OggettiRigidbody;
    public string[] Eventi;

    public TextMeshPro UISottotitolo;

    public int NumeroFrasi = 0;
    public bool NuovaFrase;

    public bool AvviaSottotitoli = true;

    void Start()
    {

        GameplayScript = GameObject.Find("# Script").GetComponent<Gameplay>();
        LocaleGeneraleScript = GameObject.Find("# Script").GetComponent<LocaleGenerale>();
        ScreenFaderScript = GameObject.Find("CenterEyeAnchor").GetComponent<BNG.ScreenFader>();


        UISottotitolo.text = "";
        Audio = this.gameObject.GetComponent<AudioSource>();
    }


    public void ProssimaFrase ( int FrasiExtra)
    {
        //Debug.Log(FrasiExtra + " - " + SubNumber);
        Audio.clip = AudioFiles[SubNumber];
        Audio.Play();
        UISottotitolo.text = SottotitoliTesti[SubNumber];
        NumeroFrasiPub = FrasiExtra;

        CallEventi(SubNumber);

        SubNumber++;
    }


    void CallEventi(int NumeroEvento)
    {
        if (Eventi[NumeroEvento] != "")
        {
            string[] ListaEventi = splitString(Eventi[NumeroEvento]);
            for (int i = 0; i < ListaEventi.Length; i++)
            {
                funzioniTutorial(ListaEventi[i]);
            }
        }
    }


    public void funzioniTutorial(string parametro)
    {

        if (parametro.Contains(","))
        {
            string[] splitArray = parametro.Split(',');

            for (int i = 0; i < splitArray.Length; i++)
            {
                ControlloSwitch(splitArray[i]);
            }
        }
        else
        {
            ControlloSwitch(parametro);
        }
    }

    void ControlloSwitch(string parametroSwitch)
    {
            switch (parametroSwitch)
            {
            case "showHighlight":

                HighlightTutorial(true,"");
                break;

            case "hideHighlight":

                HighlightTutorial(false,"");
                break;

            case "nuovoCliente":
                  GameplayScript.StartCoroutine("NuovoCliente");
                break;

            case "Instantiate":
                Instantiate(OggettiInstance[SubNumber], OggettiInstance[SubNumber].transform.position, OggettiInstance[SubNumber].transform.rotation);
                break;

            case "RigidbodyOff":

                BNG.JointHelper HelpO = OggettiRigidbody[SubNumber].GetComponent<BNG.JointHelper>();

                HelpO.LockXPosition = true;
                HelpO.LockYPosition = true;
                HelpO.LockZPosition = true;
                HelpO.LockXRotation = true;
                HelpO.LockYRotation = true;
                HelpO.LockZRotation = true;

                break;

            case "RigidbodyOn":

                Rigidbody Rig = OggettiRigidbody[SubNumber].GetComponent<Rigidbody>(); // DA SISTEMARE ASSOLUTAMENTE
                BNG.JointHelper Help = OggettiRigidbody[SubNumber].GetComponent<BNG.JointHelper>();

                if ((Rig.constraints & RigidbodyConstraints.FreezePositionX) != RigidbodyConstraints.FreezePositionX) { Help.LockXPosition = false; }
                if ((Rig.constraints & RigidbodyConstraints.FreezePositionY) != RigidbodyConstraints.FreezePositionY) { Help.LockYPosition = false; }
                if ((Rig.constraints & RigidbodyConstraints.FreezePositionZ) != RigidbodyConstraints.FreezePositionZ) { Help.LockZPosition = false; }

                if ((Rig.constraints & RigidbodyConstraints.FreezeRotationX) != RigidbodyConstraints.FreezeRotationX) { Help.LockXRotation = false; }
                if ((Rig.constraints & RigidbodyConstraints.FreezeRotationY) != RigidbodyConstraints.FreezeRotationY) { Help.LockYRotation = false; }
                if ((Rig.constraints & RigidbodyConstraints.FreezeRotationZ) != RigidbodyConstraints.FreezeRotationZ) { Help.LockZRotation = false; }

                break;

            case "RigidbodyCampanelloOn":

                BNG.JointHelper HelpC = OggettiRigidbody[6].GetComponent<BNG.JointHelper>();

                HelpC.LockXPosition = true;
                HelpC.LockYPosition = false;
                HelpC.LockZPosition = true;
                HelpC.LockXPosition = true;
                HelpC.LockYRotation = true;
                HelpC.LockZRotation = true;

                break;

            case "exitTutorial":
                StartCoroutine(AvvioLoading());
                break;

            case "payPrize":
                LocaleGeneraleScript.SoldiTotali = LocaleGeneraleScript.SoldiTotali + 2000;
                LocaleGeneraleScript.UpdateCassa();
                //LocaleGeneraleScript.SuonoAcquistoCassa(); Non metto il suono della cassa
                break;

            case "StartGameplayAfterWinner":
                GameplayScript.StartTimer();
                break;

            case "saveGame":
                ES3AutoSaveMgr.Current.Save();
                break;

            case "showComputerIcon":

                GameObject Icona1 = GameObject.Find("AllBay_Icona");
                GameObject Icona2 = GameObject.Find("Advisor_Icona");

                Icona1.GetComponent<Image>().enabled = true;
                Icona1.GetComponent<Button>().interactable = true;
                Icona1.GetComponent<Collider>().enabled = true;
                Icona1.GetComponentInChildren<Text>().enabled = true;

                Icona2.GetComponent<Image>().enabled = true;
                Icona2.GetComponent<Button>().interactable = true;
                Icona2.GetComponent<Collider>().enabled = true;
                Icona2.GetComponentInChildren<Text>().enabled = true;

                break;

            default:
                break;
        }
    }

    public void HighlightTutorialEventToFalse (string StringObject)
    {
        HighlightTutorial(false, StringObject);
    }

    void HighlightTutorial (bool Visibile, string StringObject)
    {
        if (StringObject == "")
        {
            if (OggettiEventi[SubNumber].Contains(","))
            {
                string[] splitArray = OggettiEventi[SubNumber].Split(',');

                for (int i = 0; i < splitArray.Length; i++)
                {
                    GameObject.Find(splitArray[i]).GetComponent<MeshRenderer>().enabled = Visibile;
                }
            }
            else
            {
                GameObject.Find(OggettiEventi[SubNumber]).GetComponent<MeshRenderer>().enabled = Visibile;
            }
        } else {

            if (StringObject.Contains(","))
            {
                string[] splitArray = StringObject.Split(',');

                for (int i = 0; i < splitArray.Length; i++)
                {
                    GameObject.Find(splitArray[i]).GetComponent<MeshRenderer>().enabled = Visibile;
                }
            }
            else
            {
                GameObject.Find(StringObject).GetComponent<MeshRenderer>().enabled = Visibile;
            }
        }
    }


    public string[] splitString(string Stringa)
    {
        return Stringa.Split(',');
    }


    public IEnumerator AvvioLoading()
    {
        yield return new WaitForSeconds(1);
        ScreenFaderScript.DoFadeIn();
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }


    void Update()
    {
        if (AvviaSottotitoli)
        {

            if ((!Audio.isPlaying) && (UISottotitolo.text != ""))
            {
                UISottotitolo.text = "";
                if (NumeroFrasiPub > 0)
                {
                    NumeroFrasiPub--;
                    ProssimaFrase(NumeroFrasiPub);
                }
            }

            if (NuovaFrase == true)
            {
                ProssimaFrase(NumeroFrasi);
                NuovaFrase = false;
            }

            if (timerInizio > 0)
            {
                timerInizio = timerInizio - Time.deltaTime;
            }
            else
            {
                if (timerbool == false)
                {
                    timerbool = true;
                    ProssimaFrase(2);
                }
            }

        }

    }

}
