using UnityEngine;

/// <summary>
/// Gestisce il ciclo di vita delle particelle spray/olio e applica
/// gli effetti sui capelli tramite HairManager (nessuno script sui capelli).
///
/// Effetti gestiti via OnTriggerEnter (richiede un trigger collider sulla particella):
///   Tag "SprayColor"  -> colorazione graduale del capello
///   Tag "Spray"       -> imposta HairManager.SetSpray()
///   Tag "Oil"         -> imposta HairManager.SetOil()
///   Tag "Schiuma"     -> applica mesh/materiale schiuma agli ShortHair
///   Tag "DepilSpray"  -> distrugge il capello con probabilita 1/6
///
/// Anti-attraversamento testa:
///   OnCollisionEnter con "Testa" avvia DestroyAfterDelay.
///   La particella muore prima di raggiungere l'altro lato.
///   (Stesso meccanismo del DepilSpray che funziona correttamente)
/// </summary>
public class Particelle_Hit : MonoBehaviour
{
    public bool  ModalitaOlio;
    public float movementSpeed        = 1f;
    public float distancescale        = 1f;
    public float DestructionAfterColl = 0.3f;
    public float TimerDestruction     = 1f;

    [Tooltip("Materiale da assegnare al capello (solo per tag SprayColor)")]
    public Material AssignMaterial;

    private bool dying;

    // ─────────────────────────────────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (!ModalitaOlio)
        {
            transform.position   += transform.TransformDirection(Vector3.forward) * Time.deltaTime * movementSpeed;
            transform.localScale += new Vector3(distancescale / 100f, distancescale / 100f, distancescale / 100f);
        }

        TimerDestruction -= Time.deltaTime;
        if (TimerDestruction <= 0f) Destroy(gameObject);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  COLLISIONE FISICA
    // ─────────────────────────────────────────────────────────────────────

    private void OnCollisionEnter(Collision collision)
    {
        if (dying) return;

        bool isHairOrHead = collision.collider.CompareTag("Testa")       ||
                            collision.collider.CompareTag("ShortHair")    ||
                            collision.collider.CompareTag("LongHair")     ||
                            collision.collider.CompareTag("Sopracciglia");

        if (isHairOrHead)
        {
            if (collision.collider.CompareTag("Testa"))
                StartCoroutine(DestroyAfterDelay(DestructionAfterColl));
            else
                dying = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private System.Collections.IEnumerator DestroyAfterDelay(float delay)
    {
        dying = true;
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  TRIGGER — applicazione effetti sui capelli
    // ─────────────────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        bool isHair = other.CompareTag("LongHair") ||
                      other.CompareTag("ShortHair") ||
                      other.CompareTag("Sopracciglia");
        if (!isHair) return;

        HairManager manager = other.GetComponentInParent<HairManager>();
        if (manager == null) return;

        string hairName = other.name;

        if (CompareTag("SprayColor"))
        {
            MeshRenderer mr = other.GetComponent<MeshRenderer>();
            manager.ApplySprayColor(hairName, mr, AssignMaterial);
            return;
        }

        if (CompareTag("DepilSpray"))
        {
            if (Random.Range(0, 6) == 0) Destroy(other.gameObject);
            return;
        }

        if (CompareTag("Spray"))
        {
            manager.SetSpray(hairName);
            return;
        }

        if (CompareTag("Oil"))
        {
            manager.SetOil(hairName);
            return;
        }

        if (CompareTag("Schiuma") && other.CompareTag("ShortHair"))
        {
            MeshRenderer mr = other.GetComponent<MeshRenderer>();
            MeshFilter   mf = other.GetComponent<MeshFilter>();
            if (mr != null && mf != null)
                manager.ApplySchiuma(hairName, mr, mf);
        }
    }
}
