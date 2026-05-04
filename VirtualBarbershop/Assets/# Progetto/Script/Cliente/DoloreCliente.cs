using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoloreCliente : MonoBehaviour
{
    public GameObject Testa;
    HairManager HairManagerScript;
    GameObject AudioCliente;
    AudioSource[] AudioClienteList;
    public Animator AnimatorCliente;
    public OcchiCliente OcchiClienteScript;

    public Material[] TestaConCicatrici;

    public Texture TextureTestaAlbedo;
    public Texture TextureTestaMetallic;
    public Texture TextureTestaNormal;
    public Texture TextureTestaOcclusion;

    int AudioCorrente = 0;

    // Soglie danni → indice materiale cicatrice (crescenti)
    // DanniRicevuti > soglieDanni[i] → TestaConCicatrici[i]
    private static readonly int[] soglieDanni = { 3, 6, 9, 12, 15 };

    // Componenti cached
    private Renderer testaRenderer;

    private void Start()
    {
        HairManagerScript = Testa.GetComponentInParent<HairManager>();
        AudioCliente = GameObject.Find("ClienteVoce");
        AudioClienteList = AudioCliente.GetComponentsInChildren<AudioSource>();
        testaRenderer = Testa.GetComponent<Renderer>();

        SkinnedMeshRenderer skin = GetComponentInParent<SkinnedMeshRenderer>();
        TextureTestaAlbedo    = skin.material.GetTexture("_BaseMap");
        TextureTestaMetallic  = skin.material.GetTexture("_MetallicGlossMap");
        TextureTestaNormal    = skin.material.GetTexture("_BumpMap");
        TextureTestaOcclusion = skin.material.GetTexture("_OcclusionMap");

        for (int i = 0; i < TestaConCicatrici.Length; i++)
        {
            TestaConCicatrici[i].SetTexture("_BaseMap",          TextureTestaAlbedo);
            TestaConCicatrici[i].SetTexture("_MetallicGlossMap", TextureTestaMetallic);
            TestaConCicatrici[i].SetTexture("_BumpMap",          TextureTestaNormal);
            TestaConCicatrici[i].SetTexture("_OcclusionMap",     TextureTestaOcclusion);
        }
    }

    public void Dolore()
    {
        HairManagerScript.DanniRicevuti++;

        if (!AudioClienteList[AudioCorrente].isPlaying)
        {
            AudioCorrente = Random.Range(0, AudioClienteList.Length);
            AudioClienteList[AudioCorrente].Play();
            AnimatorCliente.Play("Dolore");
            OcchiClienteScript.AnimazioneInCorso = true;
        }

        AggiornaCicatrici();
    }

    // Sostituisce la cascata di if annidati con un loop sulla tabella soglie
    private void AggiornaCicatrici()
    {
        int danni = HairManagerScript.DanniRicevuti;
        for (int i = soglieDanni.Length - 1; i >= 0; i--)
        {
            if (danni > soglieDanni[i])
            {
                testaRenderer.material = TestaConCicatrici[i];
                return;
            }
        }
    }

    public void Update()
    {
        if (OcchiClienteScript.AnimazioneInCorso &&
            !AudioClienteList[AudioCorrente].isPlaying)
        {
            OcchiClienteScript.AnimazioneInCorso = false;
        }
    }
}
