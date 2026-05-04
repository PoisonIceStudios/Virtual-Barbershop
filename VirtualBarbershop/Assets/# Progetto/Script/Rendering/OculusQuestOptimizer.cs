using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Ottimizzatore prestazioni per Meta Quest 2/3/3S.
///
/// Sistema CombinedMesh + Occlusion:
///   - I capelli sono fusi in 1-N mesh combinate (una per materiale)
///   - Per ogni mesh combinata, controlla OGNI posizione dei capelli nel gruppo
///   - Nasconde la mesh combinata SOLO se TUTTI i capelli del gruppo sono dietro la testa
///   - I capelli estratti (spray/schiuma) NON vengono toccati (sono pochi, e cambiare
///     il layer romperebbe i physics trigger dello spray)
///
/// IMPORTANTE: clientHeadCenter = TESTA DEL CLIENTE (tag "TestaOcclusione")
/// </summary>
[RequireComponent(typeof(Camera))]
public class OculusQuestOptimizer : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════════════
    //  STRUTTURA DATI PUBBLICA
    // ═══════════════════════════════════════════════════════════════════════

    public struct CombinedGroupInfo
    {
        public MeshRenderer renderer;
        public Vector3[]    hairPositions; // posizioni world dei capelli nel gruppo
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  INSPECTOR
    // ═══════════════════════════════════════════════════════════════════════

    [Header("Performance")]
    [Space(5)]
    public bool sync72Hz = true;

    [Header("Hair Occlusion")]
    [Space(5)]
    public bool EnableHairOcclusion = true;

    [Tooltip("Layer capelli VISIBILI (main camera li vede)")]
    public LayerMask HairLayer;

    [Tooltip("Layer capelli OCCLUSI (solo mirror camera li vede)")]
    public LayerMask InvisibleHairLayer;

    [Tooltip("Soglia dot-product per decidere occlusione.\n" +
             "0.0 = equatore (50% nascosti)\n" +
             "0.3 = conservativo (meno nascosti)\n" +
             "-0.1 = aggressivo (piu nascosti)")]
    [Range(-0.5f, 0.8f)]
    public float occlusionThreshold = 0.15f;

    [HideInInspector] public Transform clientHeadCenter;
    [HideInInspector] public Camera CameraRender;

    // ═══════════════════════════════════════════════════════════════════════
    //  INTERNAL STATE
    // ═══════════════════════════════════════════════════════════════════════

    private int visibleLayerId;
    private int invisibleLayerId;

    // Gruppi combinati con le posizioni dei capelli
    private readonly List<CombinedGroupInfo> combinedGroups = new List<CombinedGroupInfo>(8);

    // ═══════════════════════════════════════════════════════════════════════
    //  LIFECYCLE
    // ═══════════════════════════════════════════════════════════════════════

    private void Start()
    {
        if (sync72Hz)
            Application.targetFrameRate = 72;

        CameraRender     = GetComponent<Camera>();
        visibleLayerId   = ExtractLayerId(HairLayer);
        invisibleLayerId = ExtractLayerId(InvisibleHairLayer);

        Debug.Log($"[QuestOptimizer] Avviato | visibleLayer={visibleLayerId} invisibleLayer={invisibleLayerId}");
    }

    private void LateUpdate()
    {
        if (!EnableHairOcclusion || clientHeadCenter == null) return;
        if (combinedGroups.Count == 0) return;

        Vector3 cameraPos   = transform.position;
        Vector3 headPos     = clientHeadCenter.position;
        Vector3 camToHead   = headPos - cameraPos;
        float camHeadDist   = camToHead.magnitude;

        // Camera troppo vicina alla testa → tutto visibile
        if (camHeadDist < 0.05f)
        {
            SetAllVisible();
            return;
        }

        Vector3 camToHeadDir = camToHead / camHeadDist;

        // Per ogni gruppo combinato: controlla OGNI capello.
        // Nascondi la mesh SOLO se TUTTI i capelli sono dietro la testa.
        // 400 dot products = ~0.01ms, trascurabile anche su Quest.
        for (int g = 0; g < combinedGroups.Count; g++)
        {
            CombinedGroupInfo group = combinedGroups[g];
            if (group.renderer == null) continue;

            Vector3[] positions = group.hairPositions;
            if (positions == null || positions.Length == 0)
            {
                // Nessuna posizione cached → tieni visibile
                if (group.renderer.gameObject.layer != visibleLayerId)
                    group.renderer.gameObject.layer = visibleLayerId;
                continue;
            }

            // Controlla se ALMENO un capello e visibile (davanti alla testa)
            bool anyVisible = false;
            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 headToHair = positions[i] - headPos;
                float dist = headToHair.magnitude;

                if (dist < 0.005f)
                {
                    anyVisible = true;
                    break;
                }

                float dot = Vector3.Dot(camToHeadDir, headToHair / dist);
                if (dot <= occlusionThreshold)
                {
                    anyVisible = true;
                    break; // basta un capello visibile per tenere la mesh
                }
            }

            int targetLayer = anyVisible ? visibleLayerId : invisibleLayerId;
            if (group.renderer.gameObject.layer != targetLayer)
                group.renderer.gameObject.layer = targetLayer;
        }

        // I capelli estratti (spray/schiuma) NON vengono toccati.
        // Sono pochi (10-30 max) e cambiare il loro layer romperebbe i
        // physics trigger dello spray. Il costo GPU e trascurabile.
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  VISIBILITY HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private void SetAllVisible()
    {
        foreach (CombinedGroupInfo group in combinedGroups)
            if (group.renderer != null && group.renderer.gameObject.layer != visibleLayerId)
                group.renderer.gameObject.layer = visibleLayerId;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  REGISTRAZIONE DA HAIRMANAGER
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Chiamata da HairManager per registrare i gruppi combinati con le posizioni dei capelli.
    /// </summary>
    public void RegisterCombinedGroups(List<CombinedGroupInfo> groups)
    {
        combinedGroups.Clear();
        combinedGroups.AddRange(groups);

        // Imposta layer iniziale: tutto visibile
        int totalHairs = 0;
        foreach (CombinedGroupInfo g in groups)
        {
            if (g.renderer != null)
                g.renderer.gameObject.layer = visibleLayerId;
            totalHairs += g.hairPositions != null ? g.hairPositions.Length : 0;
        }

        Debug.Log($"[QuestOptimizer] Registrati {groups.Count} gruppi ({totalHairs} capelli)");
    }

    /// <summary>Compatibilita backward: accetta anche lista semplice di renderer.</summary>
    public void RegisterCombinedRenderers(List<MeshRenderer> renderers)
    {
        combinedGroups.Clear();
        foreach (MeshRenderer mr in renderers)
        {
            if (mr == null) continue;
            mr.gameObject.layer = visibleLayerId;
            combinedGroups.Add(new CombinedGroupInfo
            {
                renderer      = mr,
                hairPositions = new Vector3[0] // nessuna posizione = sempre visibile
            });
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  API PUBBLICA — compatibilita backward con Gameplay.cs
    // ═══════════════════════════════════════════════════════════════════════

    public void UpdateOcclusionLayer(float searchRadius)
    {
        clientHeadCenter = null;
        AutoFindClientHead();
    }

    public void UpdateOcclusionLayer(HairManager manager)
    {
        clientHeadCenter = null;
        AutoFindClientHead();
    }

    public void ResetHairVisibility()
    {
        SetAllVisible();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  AUTO-FIND TESTA CLIENTE
    // ═══════════════════════════════════════════════════════════════════════

    private void AutoFindClientHead()
    {
        if (clientHeadCenter != null) return;

        HairManager hm = FindAnyObjectByType<HairManager>();
        if (hm != null)
        {
            foreach (Transform child in hm.GetComponentsInChildren<Transform>(true))
            {
                if (child.CompareTag("TestaOcclusione"))
                {
                    clientHeadCenter = child;
                    Debug.Log($"[QuestOptimizer] clientHeadCenter auto-trovato: '{child.name}'");
                    return;
                }
            }

            GameObject testaObj = GameObject.FindWithTag("TestaOcclusione");
            if (testaObj != null)
            {
                clientHeadCenter = testaObj.transform;
                Debug.Log($"[QuestOptimizer] clientHeadCenter trovato per tag: '{testaObj.name}'");
                return;
            }

            clientHeadCenter = hm.transform;
            Debug.LogWarning($"[QuestOptimizer] Fallback testa al root: '{hm.name}'", this);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  UTILITY
    // ═══════════════════════════════════════════════════════════════════════

    public int GetVisibleLayerId() => visibleLayerId;

    private static int ExtractLayerId(LayerMask mask)
    {
        for (int i = 0; i < 32; i++)
            if (((mask >> i) & 1) == 1) return i;
        return 0;
    }
}
