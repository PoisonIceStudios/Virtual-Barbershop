using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderPosAllbay : MonoBehaviour
{

    public LocaleGenerale LocaleGeneraleScript;
    public BNG.HandController ControllerSinistro;
    public BNG.HandController ControllerDestro;
    Sottotitoli TutorialScript;

    void Start()
    {
        LocaleGeneraleScript = GameObject.Find("# Script").GetComponent<LocaleGenerale>();

        if ((ControllerSinistro == null) || (ControllerDestro == null))
        {
            Debug.LogError("Inserire le mani sui collider di bellezza AllBay");
        }

    }

    void OnTriggerEnter(Collider target)
    {
        if (target.tag == "AllBayObject")
        {
            LocaleGeneraleScript.Interni = LocaleGeneraleScript.Interni + target.GetComponent<OggettoAllBay>().BellezzaOggetto;
            // LocaleGeneraleScript.UpdateAdvisor(); forse è meglio metterlo nel giorno successivo e non in realtime
            
            if (GameObject.Find("# ScriptTutorial") != null)
            {
               TutorialScript = GameObject.Find("# ScriptTutorial").GetComponent<Sottotitoli>();

                if (TutorialScript.SubNumber == 13)
                {
                    TutorialScript.ProssimaFrase(2);
                }
            }
        }
    }

    void OnTriggerExit(Collider target)
    {
        if (target.tag == "AllBayObject")
        {
            LocaleGeneraleScript.Interni = LocaleGeneraleScript.Interni - target.GetComponent<OggettoAllBay>().BellezzaOggetto;
            //  LocaleGeneraleScript.UpdateAdvisor();  forse è meglio metterlo nel giorno successivo e non in realtime
        }
    }

}
