using System.Collections.Generic;
using UnityEngine;

public class CambiaColoreTrigger : MonoBehaviour
{
    public Material MaterialeColoreCapelli;
    [Tooltip("Velocita' di transizione colore (0=lento, 1=istantaneo)")]
    public float TempoTransizione = 1.0f;

    public ParticleSystem part;

    void Start()
    {
        part = GetComponent<ParticleSystem>();
    }

    void OnParticleCollision(GameObject CapelloTarget)
    {
        if (!CapelloTarget.CompareTag("LongHair") && !CapelloTarget.CompareTag("ShortHair")) return;

        MeshRenderer mr = CapelloTarget.GetComponent<MeshRenderer>();
        if (mr == null) return;

        // Usa HairManager per colorare correttamente (sistema CombinedMesh)
        HairManager manager = CapelloTarget.GetComponentInParent<HairManager>();
        if (manager != null)
        {
            manager.ApplySprayColor(CapelloTarget.name, mr, MaterialeColoreCapelli);
        }
    }
}
