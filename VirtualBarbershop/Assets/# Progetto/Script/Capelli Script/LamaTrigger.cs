using UnityEngine;

/// <summary>
/// Da aggiungere al GameObject del collider lama di ogni tool da taglio.
/// Gestisce tutta la logica di interazione capello-lama senza script sui capelli.
///
/// Setup (tutti i collider lama devono avere Is Trigger = true):
///   Forbice         -> aggiungi LamaTrigger a ciascun ColliderTaglio[0-3]
///   Rasoio          -> aggiungi LamaTrigger al GameObject del ColliderLama
///   RasoioElettrico -> aggiungi LamaTrigger al GameObject del ColliderLama
///
/// Il componente trova il tool padre e HairManager tramite GetComponentInParent.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LamaTrigger : MonoBehaviour
{
    private Forbice         forbice;
    private Rasoio          rasoio;
    private RasoioElettrico rasoioElettrico;
    private Rigidbody       rb;

    // Cached una volta in Start — evita GetComponent per ogni trigger
    private ParticleSystem   cachedPS;
    private ParticleSystem[] cachedPSSystems;
    private HairManager      cachedHairManager; // stesso client per tutta la sessione

    // Cooldown particelle: evita burst di Play() a raffica quando si tagliano molti capelli
    [Header("Performance Tuning")]
    [Tooltip("Secondi minimi tra un Play() e l'altro. 0 = nessun limite.")]
    public float particleCooldown = 0.05f;
    private float lastParticleTime;

    private void Start()
    {
        forbice         = GetComponentInParent<Forbice>();
        rasoio          = GetComponentInParent<Rasoio>();
        rasoioElettrico = GetComponentInParent<RasoioElettrico>();
        rb              = GetComponentInParent<Rigidbody>();
        cachedPS        = GetComponentInChildren<ParticleSystem>();
        cachedPSSystems = GetComponentsInChildren<ParticleSystem>();
    }

    // ─────────────────────────────────────────────────────────────────────
    //  TRIGGER — taglio capelli
    // ─────────────────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (forbice         != null) { HandleForbice(other);         return; }
        if (rasoioElettrico != null) { HandleRasoioElettrico(other); return; }
        if (rasoio          != null) { HandleRasoio(other); }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  LOGICA PER TOOL
    // ─────────────────────────────────────────────────────────────────────

    private void HandleForbice(Collider other)
    {
        if (!other.CompareTag("LongHair")) return;

        forbice.UtilizzaForbice();
        PlayParticlesMatchingHair(other);
        Destroy(other.gameObject);
    }

    private void HandleRasoioElettrico(Collider other)
    {
        if (!other.CompareTag("LongHair") && !other.CompareTag("Sopracciglia")) return;

        // Rasoio elettrico: 2 particelle figlie — [0] peli, [1] schiuma
        HairManager manager = FindHairManager(other);
        bool        schiuma = manager != null && manager.GetHairState(other.name).schiuma;

        if (schiuma) PlaySecondParticle();
        else         PlayParticlesMatchingHair(other);

        Destroy(other.gameObject);
    }

    private void HandleRasoio(Collider other)
    {
        if (other.CompareTag("ShortHair"))
        {
            rasoio.UtilizzoRasoio();

            // Rasoio manuale: 1 sola particella figlia — la usa sempre, con o senza schiuma
            PlayParticlesMatchingHair(other);
            Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag("LongHair"))
        {
            // Il rasoio manuale non taglia i LongHair: causa dolore se la velocita e alta
            HairManager manager  = FindHairManager(other);
            bool        schiuma  = manager != null && manager.GetHairState(other.name).schiuma;
            float       soglia   = schiuma ? 2.0f : 0.2f;
            float       velocita = rb != null ? rb.linearVelocity.magnitude : 1f;

            if (velocita > soglia)
            {
                cachedPS?.Play();
                FindDolore(other)?.Dolore();
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  HELPER
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Restituisce il HairManager del capello toccato, usando la cache per evitare
    /// GetComponentInParent ad ogni trigger. Si auto-invalida se il client viene distrutto.
    /// </summary>
    private HairManager FindHairManager(Collider hairCollider)
    {
        if (cachedHairManager != null) return cachedHairManager;
        cachedHairManager = hairCollider.GetComponentInParent<HairManager>();
        return cachedHairManager;
    }

    private void PlayParticlesMatchingHair(Collider hairCollider)
    {
        if (cachedPS == null) return;
        if (particleCooldown > 0f && Time.time - lastParticleTime < particleCooldown) return;
        lastParticleTime = Time.time;

        // Legge il colore dal HairManager (tracciato nello HairState) per EVITARE
        // mr.material che creerebbe un'istanza materiale e romperebbe lo static batch.
        Color hairColor = Color.white;
        HairManager manager = FindHairManager(hairCollider);
        if (manager != null)
        {
            hairColor = manager.GetColorCurrent(hairCollider.name);
        }
        else
        {
            // Fallback: legge da sharedMaterial (no istanza creata)
            MeshRenderer mr = hairCollider.GetComponent<MeshRenderer>();
            if (mr != null && mr.sharedMaterial != null)
            {
                Material mat = mr.sharedMaterial;
                hairColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color;
            }
        }

        var main = cachedPS.main;
        main.startColor = hairColor;
        cachedPS.Play();
    }

    private void PlaySecondParticle()
    {
        if (cachedPSSystems != null && cachedPSSystems.Length > 1)
            cachedPSSystems[1].Play();
    }

    private static DoloreCliente FindDolore(Collider hairCollider)
    {
        DoloreCliente dc = hairCollider.GetComponentInParent<DoloreCliente>();
        if (dc != null) return dc;

        GameObject testaCollider = GameObject.Find("TestaCollider");
        return testaCollider != null ? testaCollider.GetComponent<DoloreCliente>() : null;
    }
}
