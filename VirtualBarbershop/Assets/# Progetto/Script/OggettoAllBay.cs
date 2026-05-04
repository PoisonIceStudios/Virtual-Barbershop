using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OggettoAllBay : MonoBehaviour
{
    Sottotitoli TutorialScript;

    public int BellezzaOggetto = 0;

    public void StartTutorialEventToFalse(string ObjectStr)
    {
       TutorialScript = GameObject.Find("# ScriptTutorial").GetComponent<Sottotitoli>();
       TutorialScript.HighlightTutorialEventToFalse(ObjectStr);
    }
}
