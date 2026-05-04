using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{

    public string NomeScena;
    public Slider sliderBar;


    public void Start()
    {
        NomeScena = PlayerPrefs.GetString("NomeScena");
        Debug.Log (PlayerPrefs.HasKey("NomeScena"));
       // StartCoroutine(LoadNewScene(NomeScena));
    }


    IEnumerator LoadNewScene(string NomeScenaS)
    {

        yield return new WaitForSeconds(1.0f);
        AsyncOperation async = SceneManager.LoadSceneAsync(NomeScenaS);

        while (!async.isDone)
        {
            float progress = Mathf.Clamp01(async.progress / 0.9f);
            sliderBar.value = progress;
            yield return null;
        }

        yield return new WaitForSeconds(1.0f);
    }

}