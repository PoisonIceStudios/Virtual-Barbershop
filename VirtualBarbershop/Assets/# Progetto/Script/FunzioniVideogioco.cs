using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FunzioniVideogioco : MonoBehaviour
{

    // Unparent Oggetti (Esempio i tappi)
    public void Unparent(GameObject Oggetto)
    {
        Oggetto.transform.parent = null;
    }

    public void FineIntro()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }



}
