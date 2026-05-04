using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using BNG;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InGameMenu : MonoBehaviour
{
    VRUISystem VRUISystemScript;
    FunzioniMouse FunzioniMouseScript;
    ScreenFader ScreenFaderScript;
    LocaleGenerale LocaleGeneraleScript;

    Vector3 PosizioneLocale;
    Quaternion RotazioneLocale;
    bool PausaBool;
    bool FinishGame;
    GameObject OggettoPlayer;

    [Header("Menu : ")]
    [Space(10)]
    public GameObject OggettoPausa;
    public AudioSource MusicaPausa;
    [Space(5)]
    public GameObject OggettoFinish;
    public AudioSource MusicaFinish;
    [Space(5)]
    public Animator Animatorbancarotta;
    public Text testoPulsanteFinish;

    [Header("Generale : ")]
    [Space(10)]
    public GameObject OggettoPointer;

    [Header("Tasto di pausa : ")]
    [Space(10)]
    public ControllerBinding ToggleHandsInput = ControllerBinding.BackButtonDown;

    [Header("ComputerInput : ")]
    [Space(10)]
    public Canvas CanvasComputer;
    [HideInInspector]
    public bool PausaNonCosentita;

    [Header("Grabber : ")]
    [Space(10)]
    public Grabber grabberLeft;
    public Grabber grabberRight;

    [Header("Debug : ")]
    [Space(10)]
    public bool PausaDebug;
    bool IsPause;
    public bool FinishDebug;
    bool IsFinish;
    public bool NextDayDebug;
    bool IsNextDay;
    [Space(5)]
    [Tooltip("premi P per cambiare giorno (usa nuovo Input System)")]
    public bool debugDaTastiera;

    // Nuovo Input System — azione debug "NextDay" (tasto P)
    private InputAction debugNextDayAction;

    void Start()
    {
        // Registra l'azione debug P con il nuovo Input System
        debugNextDayAction = new InputAction("DebugNextDay", binding: "<Keyboard>/p");
        debugNextDayAction.Enable();

        OggettoPlayer = GameObject.FindGameObjectWithTag("Player");
        VRUISystemScript = GameObject.Find("EventSystem").GetComponent<VRUISystem>();
        FunzioniMouseScript = GameObject.Find("# Script").GetComponent<FunzioniMouse>();
        ScreenFaderScript = GameObject.Find("CenterEyeAnchor").GetComponent<ScreenFader>();
        LocaleGeneraleScript = GameObject.Find("# Script").GetComponent<LocaleGenerale>();

        grabberLeft = GameObject.FindGameObjectWithTag("GrabberLeft").GetComponent<Grabber>();
        grabberRight = GameObject.FindGameObjectWithTag("GrabberRight").GetComponent<Grabber>();

        CanvasComputer.worldCamera = FunzioniMouseScript.CameraCursore;

        OggettoPointer.SetActive(false);
        VRUISystemScript.enabled = false;
    }

    void Update()
    {

        if (debugDaTastiera)
        {
            if (debugNextDayAction != null && debugNextDayAction.WasPressedThisFrame())
            {
                NextDayDebug = true;
            }
        }

        if (PausaDebug == true)
        {
            if (IsPause == false)
            {
                IsPause = true;
                PausaOn();
                PausaDebug = false;
            } else
            {
                IsPause = false;
                PausaOff();
                PausaDebug = false;
            }
        }

        if (FinishDebug == true)
        {
            if (IsFinish == false)
            {
                IsFinish = true;
                FinishDay();
            }

        }

        if (NextDayDebug == true)
        {
            if (IsNextDay == false)
            {
                IsNextDay = true;
                GiornoSuccessivo();
            }
        }

        if ((ToggleHandsInput.GetDown()) && (PausaBool == false) && (FinishGame == false))
        {
            
            PausaOn();

        } else if ((ToggleHandsInput.GetDown()) && (PausaBool == true) && (FinishGame == false))
        {
            PausaOff();
        }

    }

    private void OnDestroy()
    {
        debugNextDayAction?.Disable();
        debugNextDayAction?.Dispose();
    }

    public void PausaOn()
    {
        if (PausaNonCosentita == false)
        {
            PausaBool = true;

            grabberLeft.TryRelease();
            grabberRight.TryRelease();

            OggettoPlayer.GetComponent<CharacterController>().enabled = false;

            MusicaPausa.Play();

            PosizioneLocale = OggettoPlayer.transform.position;
            RotazioneLocale = OggettoPlayer.transform.localRotation;

            OggettoPlayer.transform.position = new Vector3(OggettoPausa.transform.position.x, OggettoPlayer.transform.position.y, OggettoPausa.transform.position.z);
            OggettoPlayer.transform.localRotation = new Quaternion(OggettoPausa.transform.localRotation.x, ( OggettoPausa.transform.localRotation.y -180), OggettoPausa.transform.localRotation.z, OggettoPausa.transform.localRotation.w);

            OggettoPointer.SetActive(true);
            if (CanvasComputer != null)
            {
                VRUISystemScript.enabled = true;
                CanvasComputer.worldCamera = OggettoPointer.GetComponentInChildren<Camera>();
            }
        }
    }

    public void PausaOff()
    {

        if (PausaNonCosentita == false)
        {

            PausaBool = false;

            OggettoPlayer.GetComponent<CharacterController>().enabled = true;

            MusicaPausa.Stop();

            OggettoPlayer.transform.position = PosizioneLocale;
            OggettoPlayer.transform.localRotation = RotazioneLocale;

            OggettoPointer.SetActive(false);
            if (CanvasComputer != null)
            {
                VRUISystemScript.enabled = false;
                CanvasComputer.worldCamera = FunzioniMouseScript.CameraCursore;
            }
        }
    }

    public void FinishDay()
    {
        if (PausaNonCosentita == false)
        {


            FinishGame = true;

            OggettoPlayer.GetComponent<CharacterController>().enabled = false;

            grabberLeft.TryRelease();
            grabberRight.TryRelease();

            MusicaFinish.Play();

            PosizioneLocale = OggettoPlayer.transform.position;
            RotazioneLocale = OggettoPlayer.transform.localRotation;

            OggettoPlayer.transform.position = new Vector3(OggettoFinish.transform.position.x, OggettoPlayer.transform.position.y, OggettoFinish.transform.position.z);
            OggettoPlayer.transform.localRotation = new Quaternion(OggettoFinish.transform.localRotation.x, (OggettoFinish.transform.localRotation.y - 180), OggettoFinish.transform.localRotation.z, OggettoFinish.transform.localRotation.w);

            OggettoPointer.SetActive(true); 

            if (CanvasComputer != null)
            {
                VRUISystemScript.enabled = true;
                CanvasComputer.worldCamera = OggettoPointer.GetComponentInChildren<Camera>();
            }

            if (LocaleGeneraleScript.SoldiTotali <= 0) //Bancarotta
            {
                testoPulsanteFinish.text = "Main Men�";
                Animatorbancarotta.enabled = true;
            }

        }
    }

    public void GiornoSuccessivo()
    {
        StartCoroutine("GiornoSuccessivoCoroutine");
    }

    public IEnumerator GiornoSuccessivoCoroutine()
    {
        ES3AutoSaveMgr.Current.Save();

        ScreenFaderScript.DoFadeIn();
        yield return new WaitForSeconds(1f);


        if (LocaleGeneraleScript.SoldiTotali <= 0)
        {
            ES3.DeleteFile("Game.es3");
            SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene("BarberShopCarrerMode", LoadSceneMode.Single);
        }


    }
}
