using UnityEngine;

public class Automobili : MonoBehaviour
{
    public Renderer[] Auto;
    public Color[] Colori;

    // Cached per evitare allocazioni
    private static readonly int ColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorLegacyID = Shader.PropertyToID("_Color");
    private MaterialPropertyBlock mpb;

    public void Start()
    {
        mpb = new MaterialPropertyBlock();
    }

    public void CambiaColoreAuto()
    {
        for (int i = 0; i < Auto.Length; i++)
        {
            Auto[i].enabled = true;
            Color colore = Colori[Random.Range(0, Colori.Length)];

            // MaterialPropertyBlock: cambia colore senza creare istanze materiale
            Auto[i].GetPropertyBlock(mpb);
            mpb.SetColor(ColorID, colore);
            mpb.SetColor(ColorLegacyID, colore);
            Auto[i].SetPropertyBlock(mpb);
        }
    }
}
