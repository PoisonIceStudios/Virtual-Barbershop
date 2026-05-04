using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using BNG;


public class UIMenu : MonoBehaviour
{

    BNG.PlayerTeleport teleport;
    BNG.SmoothLocomotion smoothLocomotion;

    BNG.ScreenFader ScreenFaderScript;

    LocomotionType selectedLocomotion = LocomotionType.Teleport;

    public bool SoloPausaMenu;
    public bool TestCaricaScena;


    public Canvas[] CanvasMenu;
    public Canvas[] Game;
    public Canvas[] Settings;

    public UnityEngine.UI.Button ContinueButton;

    public float TimerFade = 1f;

    [Header("Popup UI : ")]
    [Space(10)]
    public Canvas[] PopupCanvas;

    [Header("UI Settings : ")]
    [Space(10)]
    public Toggle HeightModeToggle;
    public Toggle SeatedModeToggle;
    [Space(4)]
    public Toggle TurnSnapToggle;
    public Toggle TurnSmoothToggle;
    [Space(4)]
    public Toggle FreeMovementsToggle;
    public Toggle TeleportModeToggle;
    [Space(4)]
    public Toggle RemoteGrabberONToggle;
    public Toggle RemoteGrabberOFFToggle;
    [Space(4)]
    public UnityEngine.UI.Slider VolumeSlider;
    [Space(4)]
    public Toggle EnableSubtitleToggle;

    [Header("Version : ")]
    [Space(10)]
    public Text VersionText;

    // Cache PlayerPrefs (evita I/O ripetuto)
    private int pref_HeightMode;
    private int pref_TurnMode;
    private int pref_MovementsMode;
    private int pref_EnableRemoteGrabber;
    private int pref_EnableSubtitle;
    private float pref_Volume;



    [Header("LoadingZone : ")]
    [Space(10)]
    public UnityEngine.UI.Slider sliderBarLoading;

    void Start()
    {

        if (sliderBarLoading == null)
        {
            
            ScreenFaderScript = GameObject.Find("CenterEyeAnchor").GetComponent<BNG.ScreenFader>();

            ScreenFaderScript.FadeInSpeed = TimerFade;
            ScreenFaderScript.FadeOutSpeed = TimerFade;
            ScreenFaderScript.FadeColor = new Color32(0, 0, 0, 255);



                if (SceneManager.GetActiveScene().name != "Loading")
                {

                    LoadSettingsValue();

                    if (VersionText != null)
                    {
                        VersionText.text = "Version: " + Application.version;
                    }


                    if (SceneManager.GetActiveScene().name == "Menu")
                    {

                        if (ES3.FileExists("Game.es3"))
                        {
                            ContinueButton.interactable = true;
                        }

                    }
                }
              
        }
        else  // Se sono solo per il loading
        {
            StartCoroutine(LoadingSceneAsync(ReadSceneforLoading()));
        }
    }

    // ------------------ Funzioni ------------------//

    public void Loading(string NomeScena)
    {
        if (!SoloPausaMenu)
        {
            SaveSettingsValue();
        }

        SaveSceneforLoading(NomeScena);
        StartCoroutine(LoadingScene("Loading"));
    }

    public void FadeOn()
    {
        ScreenFaderScript.DoFadeIn();
    }

    public void FadeOff()
    {
        ScreenFaderScript.DoFadeOut();
    }

    public void CancellaSalvataggio(string Nome)
    {
        ES3.DeleteFile(Nome);
    }

    public void MostraPopup(int Popup)
    {
        PopupCanvas[Popup].enabled = true;
    }

    public void ChiudiPopup(int Popup)
    {
        PopupCanvas[Popup].enabled = false;
    }

    public void EsciGioco()
    {
        Application.Quit();
    }

    public void DeletePrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    public void MostraCanvasMenu(int NumeroCanvas)
    {

        for (int i = 0; i < CanvasMenu.Length; i++)
        {
            if (i == NumeroCanvas)
            {
                CanvasMenu[i].enabled = true;
            }
            else
            {
                CanvasMenu[i].enabled = false;
            }
        }
    }
    public void MostraCanvasGame(int NumeroCanvas)
    {

        for (int i = 0; i < Game.Length; i++)
        {
            if (i == NumeroCanvas)
            {
                Game[i].enabled = true;
            }
            else
            {
                Game[i].enabled = false;
            }
        }
    }

    public void MostraCanvasSettings(int NumeroCanvas)
    {

        for (int i = 0; i < Settings.Length; i++)
        {
            if (i == NumeroCanvas)
            {
                Settings[i].enabled = true;
            }
            else
            {
                Settings[i].enabled = false;
            }
        }
    }


    //---------------- Loading Scene -----------------//
    IEnumerator LoadingScene(string Scene)  
    {
        FadeOn();
        yield return new WaitForSeconds(TimerFade);
        SceneManager.LoadScene("Loading", LoadSceneMode.Single);
    }

    IEnumerator LoadingSceneAsync(string Scene)
    {
        yield return new WaitForSeconds(1.0f);
        AsyncOperation async = SceneManager.LoadSceneAsync(Scene);

        while (!async.isDone)
        {
            float progress = Mathf.Clamp01(async.progress / 0.9f);
            sliderBarLoading.value = progress;
            yield return null;
        }

        yield return new WaitForSeconds(1.0f);
    }



    // ----------------- PlayerPrefs -------------------//


    public void SaveSceneforLoading(string NomeScena)
    {
        PlayerPrefs.SetString("NomeScena", NomeScena);
    }

    public string ReadSceneforLoading()
    {
        string ret = PlayerPrefs.GetString("NomeScena");
        return ret;
    }


    public void SaveSettingsValue()
    {

        if (HeightModeToggle.isOn) { PlayerPrefs.SetInt("HeightMode", 0); } else { PlayerPrefs.SetInt("HeightMode", 1); }
        if (TurnSnapToggle.isOn) { PlayerPrefs.SetInt("TurnMode", 0); } else { PlayerPrefs.SetInt("TurnMode", 1); }
        if (FreeMovementsToggle.isOn) { PlayerPrefs.SetInt("MovementsMode", 0); } else { PlayerPrefs.SetInt("MovementsMode", 1); }
        if (RemoteGrabberONToggle.isOn) { PlayerPrefs.SetInt("EnableRemoteGrabber", 0); } else { PlayerPrefs.SetInt("EnableRemoteGrabber", 1); }

        if (EnableSubtitleToggle.isOn) { PlayerPrefs.SetInt("EnableSubtitle", 0); } else { PlayerPrefs.SetInt("EnableSubtitle", 1); }
        PlayerPrefs.SetFloat("Volume", VolumeSlider.value);

    }

    public void LoadSettingsValue()
    {
        // Leggi tutti i valori una sola volta
        pref_HeightMode          = PlayerPrefs.GetInt("HeightMode");
        pref_TurnMode            = PlayerPrefs.GetInt("TurnMode");
        pref_MovementsMode       = PlayerPrefs.GetInt("MovementsMode");
        pref_EnableRemoteGrabber = PlayerPrefs.GetInt("EnableRemoteGrabber");
        pref_EnableSubtitle      = PlayerPrefs.GetInt("EnableSubtitle");

        if (SceneManager.GetActiveScene().name == "Menu") //Set UI
        {
            if (pref_HeightMode == 1) { SeatedModeToggle.isOn = true; } else { HeightModeToggle.isOn = true; }
            if (pref_TurnMode == 1) { TurnSmoothToggle.isOn = true; } else { TurnSnapToggle.isOn = true; }
            if (pref_MovementsMode == 1) { TeleportModeToggle.isOn = true; } else { FreeMovementsToggle.isOn = true; }
            if (pref_EnableRemoteGrabber == 1) { RemoteGrabberOFFToggle.isOn = true; } else { RemoteGrabberONToggle.isOn = true; }
            if (pref_EnableSubtitle == 1) { EnableSubtitleToggle.isOn = false; }

            if (!PlayerPrefs.HasKey("Volume"))
            {
                PlayerPrefs.SetFloat("Volume", 1f);
                VolumeSlider.value = 1f;
            }
            else
            {
                VolumeSlider.value = PlayerPrefs.GetFloat("Volume");
            }

        }

        if (pref_HeightMode == 1) { SetPlayerHeight(0.4f); } else { SetPlayerHeight(-0.025f); }
        if (pref_TurnMode == 1) { SetTurningMode(1); } else { SetTurningMode(0); }
        if (pref_MovementsMode == 1) { SetMovementsMode(1); } else { SetMovementsMode(0); }
        if (pref_EnableRemoteGrabber == 1) { RemoteGrabber(1); }

        AudioListener.volume = PlayerPrefs.GetFloat("Volume");

        if ((SceneManager.GetActiveScene().name == "BarberShopCarrerMode") || (SceneManager.GetActiveScene().name == "BarberShoFreeMode") || (SceneManager.GetActiveScene().name == "BarberShopTutorial"))
        {
            if (pref_EnableSubtitle == 1) { GameObject.Find("SubtitlesText").GetComponent<TextMeshPro>().enabled = false; }
        }
               
    }


    void OnApplicationPause(bool pause)
    {
        if (SceneManager.GetActiveScene().name == "Menu" && pause == true) 
        {
            SaveSettingsValue(); 
        }
    }

    private void OnApplicationQuit()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            SaveSettingsValue();
        }
    }

    public void changeVolume(UnityEngine.UI.Slider newVolume)
    {
        AudioListener.volume = newVolume.value;
    }

    public void SetPlayerHeight(float Valore)
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<BNG.BNGPlayerController>().CharacterControllerYOffset = Valore;
    }


    public void SetTurningMode(int Modalita)
    {

            if (Modalita != 0)
            {
               GameObject.FindGameObjectWithTag("Player").GetComponent<BNG.PlayerRotation>().RotationType = BNG.RotationMechanic.Smooth;
            }
            else
            {
               GameObject.FindGameObjectWithTag("Player").GetComponent<BNG.PlayerRotation>().RotationType = BNG.RotationMechanic.Snap;
            }
    }

    public void SetMovementsMode(int Modalita)
    {
        if (SceneManager.GetActiveScene().name != "Menu")
        {
            if (Modalita != 0)
            {
                ChangeLocomotionType(LocomotionType.Teleport);
                GameObject.FindGameObjectWithTag("Player").GetComponent<BNG.SmoothLocomotion>().enabled = false;
                GameObject.FindGameObjectWithTag("Player").GetComponent<BNG.PlayerTeleport>().enabled = true;
            }
            else
            {
                ChangeLocomotionType(LocomotionType.SmoothLocomotion);
                GameObject.FindGameObjectWithTag("Player").GetComponent<BNG.SmoothLocomotion>().enabled = true;
                GameObject.FindGameObjectWithTag("Player").GetComponent<BNG.PlayerTeleport>().enabled = false;
            }
        }
    }

    public void RemoteGrabber(int Modalita)
    {
        if (Modalita != 0)
        {
            Destroy(GameObject.FindGameObjectWithTag("RemoteGrabberLeft"));
            Destroy(GameObject.FindGameObjectWithTag("RemoteGrabberRight"));
        }

    }


    // ------------ Change locomotion e rotation type ------------------//

    public void ChangeLocomotionType(LocomotionType loc)
    {
        selectedLocomotion = loc;

        if (smoothLocomotion == null)
        {
            smoothLocomotion = GameObject.FindGameObjectWithTag("Player").GetComponent<SmoothLocomotion>();
        }

        if (teleport == null)
        {
            teleport = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerTeleport>();
        }

        toggleTeleport(selectedLocomotion == LocomotionType.Teleport);
        toggleSmoothLocomotion(selectedLocomotion == LocomotionType.SmoothLocomotion);
    }

    void toggleTeleport(bool enabled)
    {
        if (enabled)
        {
            teleport.EnableTeleportation();
        }
        else
        {
            teleport.DisableTeleportation();
        }
    }

    void toggleSmoothLocomotion(bool enabled)
    {
        if (smoothLocomotion) { smoothLocomotion.enabled = enabled; }
    }




}
