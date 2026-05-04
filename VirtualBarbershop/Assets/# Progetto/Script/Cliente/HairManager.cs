using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
#endif

/// <summary>
/// Manager centrale del cliente: gestisce lo stato runtime di ogni capello
/// e li combina in poche mesh unite per prestazioni ottimali su Quest.
///
/// SISTEMA COMBINED MESH:
///   - All'Awake, tutti i capelli vengono fusi in UNA mesh per materiale
///     tramite Mesh.CombineMeshes (vera fusione, 1 draw call per materiale)
///   - I MeshRenderer individuali vengono DISABILITATI (i Collider restano attivi)
///   - Quando un capello viene tagliato (Destroy), il LateUpdate rileva il null
///     e ricostruisce la mesh combinata del gruppo
///   - Quando un capello viene spruzzato, viene estratto dalla mesh combinata
///     e mostrato individualmente con MaterialPropertyBlock per il colore
///   - Quando lo spray è completo, il capello viene ri-fuso con il nuovo materiale
/// </summary>
public class HairManager : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────
    //  STATO PER CAPELLO
    // ─────────────────────────────────────────────────────────────────────

    public class HairState
    {
        public bool     spray;
        public bool     oil;
        public bool     schiuma;
        public float    colorFade;
        public Material targetMat;
        public Color    colorBase;
        public Color    colorCurrent;
    }

    private readonly Dictionary<string, HairState> hairStates = new();

    private HairState GetOrCreateState(string hairName)
    {
        if (!hairStates.TryGetValue(hairName, out HairState state))
        {
            state = new HairState();
            hairStates[hairName] = state;
        }
        return state;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  API PUBBLICA — chiamata da LamaTrigger e Particelle_Hit
    // ─────────────────────────────────────────────────────────────────────

    public HairState GetHairState(string hairName) => GetOrCreateState(hairName);
    public Color     GetColorBase(string hairName)    => GetOrCreateState(hairName).colorBase;
    public Color     GetColorCurrent(string hairName) => GetOrCreateState(hairName).colorCurrent;

    public void SetSpray(string hairName) => GetOrCreateState(hairName).spray = true;
    public void SetOil(string hairName)   => GetOrCreateState(hairName).oil   = true;

    public void ApplySchiuma(string hairName, MeshRenderer mr, MeshFilter mf)
    {
        if (meshSchiuma == null || materialeSchiuma == null) return;
        HairState s = GetOrCreateState(hairName);
        if (s.schiuma) return;

        // Estrai il capello dalla mesh combinata per mostrarlo con schiuma
        if (hairNameToIndex.TryGetValue(hairName, out int idx))
            ExtractHairFromCombined(idx);

        mr.sharedMaterial = materialeSchiuma;
        mf.mesh           = meshSchiuma;
        s.schiuma         = true;
        s.colorCurrent    = ReadMaterialColor(materialeSchiuma);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  SPRAY COLORING — con MaterialPropertyBlock (zero istanze materiale)
    // ─────────────────────────────────────────────────────────────────────

    private static MaterialPropertyBlock s_sprayMPB;
    private static int s_BaseColorID;
    private static int s_ColorID;
    private static bool s_mpbInitialized;

    private static void EnsureMPB()
    {
        if (s_mpbInitialized) return;
        s_sprayMPB       = new MaterialPropertyBlock();
        s_BaseColorID    = Shader.PropertyToID("_BaseColor");
        s_ColorID        = Shader.PropertyToID("_Color");
        s_mpbInitialized = true;
    }

    public void ApplySprayColor(string hairName, MeshRenderer mr, Material assignMat)
    {
        if (mr == null || assignMat == null) return;
        HairState s = GetOrCreateState(hairName);
        if (s.schiuma) return;

        // Se gia completato con questo materiale, non fare nulla
        if (s.spray && s.targetMat == assignMat) return;

        // Primo hit spray: estrai il capello dalla mesh combinata per mostrarlo individualmente
        if (hairNameToIndex.TryGetValue(hairName, out int idx))
            ExtractHairFromCombined(idx);

        // Cambio colore spray: reset fade
        if (s.targetMat != assignMat)
        {
            s.colorFade = 0f;
            s.targetMat = assignMat;
        }

        s.colorFade = Mathf.Clamp01(s.colorFade + colorStep);

        // Colore pieno raggiunto: switcha al MATERIALE REALE e ri-fondi
        if (s.colorFade >= 1f)
        {
            mr.SetPropertyBlock(null);
            mr.sharedMaterial = assignMat;
            s.colorCurrent    = ReadMaterialColor(assignMat);
            s.spray           = true;

            if (hairNameToIndex.TryGetValue(hairName, out int remergeIdx))
                RemergeHairIntoCombined(remergeIdx, assignMat);
            return;
        }

        // Progressione intermedia: lerp dal colore BASE per progressione lineare
        Color newColor = lerpHSB
            ? LerpViaHSB(s.colorBase, ReadMaterialColor(assignMat), s.colorFade)
            : Color.Lerp(s.colorBase, ReadMaterialColor(assignMat), s.colorFade);

        // MaterialPropertyBlock: cambia il colore SENZA istanze materiale
        EnsureMPB();
        mr.GetPropertyBlock(s_sprayMPB);
        s_sprayMPB.SetColor(s_BaseColorID, newColor);
        s_sprayMPB.SetColor(s_ColorID, newColor);
        mr.SetPropertyBlock(s_sprayMPB);

        s.colorCurrent = newColor;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  COMBINED MESH SYSTEM — vera fusione in UN blocco
    // ─────────────────────────────────────────────────────────────────────

    private struct CachedHairData
    {
        public Mesh       mesh;
        public Matrix4x4  localMatrix;
        public Material   material;
        public bool       destroyed;
        public bool       extracted; // estratto per spray/schiuma
    }

    private class CombinedGroup
    {
        public Material      material;
        public GameObject    gameObject;
        public MeshFilter    meshFilter;
        public MeshRenderer  meshRenderer;
        public List<int>     hairIndices;
        public bool          dirty;
    }

    private CachedHairData[]      cachedHairData;
    private MeshRenderer[]        individualRenderers;
    private List<CombinedGroup>   combinedGroups = new();
    private Dictionary<string, int>           hairNameToIndex  = new();
    private Dictionary<string, CombinedGroup> materialToGroup  = new();
    private Dictionary<int, CombinedGroup>    hairIndexToGroup = new(); // O(1) per idx, no string alloc
    private OculusQuestOptimizer  cachedOptimizer;

    // ── Combined mesh: chunk size ────────────────────────────────────────
    [Header("Performance Tuning")]
    [Tooltip("Capelli per chunk combinato. Meno = rebuild più veloce, più draw call.")]
    public int maxHairsPerGroup = 100;


    /// <summary>
    /// Combina tutti i capelli in poche mesh unite (una per materiale).
    /// Disabilita i MeshRenderer individuali. I Collider restano attivi.
    /// </summary>
    private void BuildCombinedMeshes()
    {
        if (objectLength == 0) return;

        cachedHairData      = new CachedHairData[objectLength];
        individualRenderers = new MeshRenderer[objectLength];
        Matrix4x4 rootInverse = transform.worldToLocalMatrix;

        // Cache dati e disabilita renderer individuali
        for (int i = 0; i < objectLength; i++)
        {
            if (meshObject[i] == null) continue;

            MeshFilter   mf = meshObject[i].GetComponent<MeshFilter>();
            MeshRenderer mr = meshObject[i].GetComponent<MeshRenderer>();
            if (mf == null || mr == null || mf.sharedMesh == null) continue;

            cachedHairData[i] = new CachedHairData
            {
                mesh        = mf.sharedMesh,
                localMatrix = rootInverse * mf.transform.localToWorldMatrix,
                material    = mr.sharedMaterial,
                destroyed   = false,
                extracted   = false
            };

            individualRenderers[i] = mr;
            mr.enabled = false;    // nascosto — la mesh combinata renderizza

            hairNameToIndex[meshObject[i].name] = i;
        }

        // Raggruppa per materiale (prima fase: solo indici)
        var indicesByMat = new Dictionary<string, List<int>>();
        var matByName    = new Dictionary<string, Material>();
        for (int i = 0; i < objectLength; i++)
        {
            if (cachedHairData[i].mesh == null) continue;
            string matName = cachedHairData[i].material != null ? cachedHairData[i].material.name : "null";
            if (!indicesByMat.TryGetValue(matName, out List<int> list))
            {
                list = new List<int>();
                indicesByMat[matName] = list;
                matByName[matName]    = cachedHairData[i].material;
            }
            list.Add(i);
        }

        // Crea mesh combinata in CHUNK da maxHairsPerGroup per materiale.
        // Con 500 capelli/materiale e chunk da 100: 5 draw call, ma ogni rebuild costa 5x meno.
        foreach (var kvp in indicesByMat)
        {
            string     matName    = kvp.Key;
            List<int>  allIndices = kvp.Value;
            Material   mat        = matByName[matName];

            for (int start = 0; start < allIndices.Count; start += maxHairsPerGroup)
            {
                int count = Mathf.Min(maxHairsPerGroup, allIndices.Count - start);

                CombinedGroup chunk = new CombinedGroup
                {
                    material    = mat,
                    hairIndices = allIndices.GetRange(start, count)
                };

                GameObject go = new GameObject($"_Hair_{matName}_{start / maxHairsPerGroup}");
                go.transform.SetParent(transform, false);

                chunk.gameObject    = go;
                chunk.meshFilter    = go.AddComponent<MeshFilter>();
                chunk.meshRenderer  = go.AddComponent<MeshRenderer>();
                chunk.meshRenderer.sharedMaterial    = mat;
                chunk.meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                chunk.meshRenderer.receiveShadows    = false;

                RebuildGroupMesh(chunk);
                combinedGroups.Add(chunk);
                materialToGroup[matName] = chunk; // punta sempre all'ultimo chunk (per Remerge)

                foreach (int idx in chunk.hairIndices)
                    hairIndexToGroup[idx] = chunk;
            }
        }

        int totalGroups = combinedGroups.Count;
        Debug.Log($"[{nomeCliente}] CombinedMesh: {objectLength} capelli -> {totalGroups} draw call" +
                  (totalGroups > 1 ? "s" : "") + $" (chunk da {maxHairsPerGroup})");

        // Registra le mesh combinate con l'optimizer per il layer corretto
        RegisterCombinedWithOptimizer();
    }

    private void RebuildGroupMesh(CombinedGroup group)
    {
        List<CombineInstance> combines = new();

        foreach (int idx in group.hairIndices)
        {
            if (cachedHairData[idx].destroyed || cachedHairData[idx].extracted) continue;
            if (cachedHairData[idx].mesh == null) continue;

            combines.Add(new CombineInstance
            {
                mesh      = cachedHairData[idx].mesh,
                transform = cachedHairData[idx].localMatrix
            });
        }

        if (combines.Count == 0)
        {
            if (group.meshFilter.sharedMesh != null)
            {
                Destroy(group.meshFilter.sharedMesh);
                group.meshFilter.sharedMesh = null;
            }
            group.gameObject.SetActive(false);
            group.dirty = false;
            return;
        }

        Mesh combined = new Mesh();
        combined.indexFormat = IndexFormat.UInt32; // supporta > 65K vertici
        combined.CombineMeshes(combines.ToArray(), true, true);
        combined.RecalculateBounds();

        Mesh old = group.meshFilter.sharedMesh;
        group.meshFilter.sharedMesh = combined;
        if (old != null) Destroy(old);

        group.gameObject.SetActive(true);
        group.dirty = false;
    }

    /// <summary>Estrae un capello dalla mesh combinata per mostrarlo individualmente.</summary>
    private void ExtractHairFromCombined(int idx)
    {
        if (cachedHairData == null || idx >= cachedHairData.Length) return;
        if (cachedHairData[idx].extracted || cachedHairData[idx].destroyed) return;

        cachedHairData[idx].extracted = true;

        // Ri-abilita il renderer individuale (il layer NON viene toccato
        // per non rompere i physics trigger dello spray)
        if (individualRenderers[idx] != null)
            individualRenderers[idx].enabled = true;

        // Marca il gruppo come sporco -> rebuild al prossimo LateUpdate
        if (hairIndexToGroup.TryGetValue(idx, out CombinedGroup group))
            group.dirty = true;
    }

    /// <summary>Ri-fondi un capello nella mesh combinata dopo spray completo.</summary>
    private void RemergeHairIntoCombined(int idx, Material newMaterial)
    {
        if (cachedHairData == null || idx >= cachedHairData.Length) return;
        if (!cachedHairData[idx].extracted) return;

        // Disabilita renderer individuale (torna nella mesh combinata)
        if (individualRenderers[idx] != null)
            individualRenderers[idx].enabled = false;

        cachedHairData[idx].extracted = false;

        // Se il materiale è cambiato, sposta il capello nel nuovo gruppo/chunk
        string oldMatName = cachedHairData[idx].material != null ? cachedHairData[idx].material.name : "null";
        string newMatName = newMaterial != null ? newMaterial.name : "null";

        if (oldMatName != newMatName)
        {
            // Rimuovi dal vecchio chunk (lookup diretto per idx)
            if (hairIndexToGroup.TryGetValue(idx, out CombinedGroup oldGroup))
            {
                oldGroup.hairIndices.Remove(idx);
                oldGroup.dirty = true;
            }

            // Aggiungi al chunk del nuovo materiale (cerca uno con spazio o crea)
            cachedHairData[idx].material = newMaterial;
            CombinedGroup targetChunk = null;

            if (materialToGroup.TryGetValue(newMatName, out CombinedGroup lastChunk))
            {
                if (lastChunk.hairIndices.Count < maxHairsPerGroup)
                    targetChunk = lastChunk;
            }

            if (targetChunk == null)
            {
                targetChunk = new CombinedGroup
                {
                    material    = newMaterial,
                    hairIndices = new List<int>()
                };

                GameObject go = new GameObject($"_Hair_{newMatName}");
                go.transform.SetParent(transform, false);
                targetChunk.gameObject   = go;
                targetChunk.meshFilter   = go.AddComponent<MeshFilter>();
                targetChunk.meshRenderer = go.AddComponent<MeshRenderer>();
                targetChunk.meshRenderer.sharedMaterial      = newMaterial;
                targetChunk.meshRenderer.shadowCastingMode   = ShadowCastingMode.Off;
                targetChunk.meshRenderer.receiveShadows      = false;

                combinedGroups.Add(targetChunk);
                materialToGroup[newMatName] = targetChunk;

                RegisterCombinedWithOptimizer();
            }

            targetChunk.hairIndices.Add(idx);
            targetChunk.dirty = true;
            hairIndexToGroup[idx] = targetChunk;
        }
        else
        {
            // Stesso materiale: riusa il chunk originale
            if (hairIndexToGroup.TryGetValue(idx, out CombinedGroup group))
                group.dirty = true;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  LATEUPDATE — rileva capelli tagliati e ricostruisce mesh combinata
    // ─────────────────────────────────────────────────────────────────────

    // Chunked scan: controlla N capelli per frame invece di tutti
    private int destroyScanIndex;
    private const int DESTROY_SCAN_PER_FRAME = 80;

    private void LateUpdate()
    {
        if (cachedHairData == null) return;

        // ── Chunked scan — fallback per Destroy() chiamati fuori da MarkHairDeleted ──
        int scanEnd = Mathf.Min(destroyScanIndex + DESTROY_SCAN_PER_FRAME, objectLength);
        for (int i = destroyScanIndex; i < scanEnd; i++)
        {
            if (cachedHairData[i].mesh == null || cachedHairData[i].destroyed) continue;
            if (meshObject[i] == null)
            {
                cachedHairData[i].destroyed = true;
                if (hairIndexToGroup.TryGetValue(i, out CombinedGroup fallbackGroup))
                    fallbackGroup.dirty = true;
            }
        }
        destroyScanIndex = scanEnd >= objectLength ? 0 : scanEnd;

        // Ricostruisci solo i gruppi sporchi (spray, schiuma, capelli tagliati)
        foreach (CombinedGroup group in combinedGroups)
        {
            if (group.dirty)
                RebuildGroupMesh(group);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  INSPECTOR
    // ─────────────────────────────────────────────────────────────────────

    [Header("Tag Capelli")]
    public string tagCapelliLunghi = "LongHair";
    public string tagCapelliCorti  = "ShortHair";
    public string tagSopracciglia  = "Sopracciglia";

    [Header("Stile Obiettivo")]
    public HairStyleSO hairStyle;

    [Header("Materiali Capelli")]
    public Material[] hairMaterials;

    [HideInInspector] public List<GameObject> meshObject = new List<GameObject>();
    [HideInInspector] public GameObject[] sopracciglia;

    [Header("Nome Cliente")]
    public string nomeCliente = "Cliente";

    [Header("Schiuma Globale")]
    public Mesh meshSchiuma;
    public Material materialeSchiuma;

    [Header("Colorazione Spray")]
    [Tooltip("Quanto colore aggiunge ogni hit spray (0.25 = 4 hit per colore pieno, 0.5 = 2 hit, 1.0 = istantaneo)")]
    public float colorStep = 0.25f;
    public bool lerpHSB;

    [Header("Danni")]
    public int DanniRicevuti = 0;

    [Header("Pagamento")]
    public Animator Animazione;
    public int pagaBaseCliente = 10;
    public TextMesh ParticelleTestoSoldi;

    [Header("Static Batching (legacy — ora usa CombinedMesh)")]
    public GameObject RootCapelli;

    [Header("Sistema Foto (Editor Only)")]
    public GameObject SistemaPerFoto;
    public Color32[] ColoriMagliette;

    [HideInInspector] public float punteggioTotaleInizio;
    [HideInInspector] public int   punteggioTotale;

    private LocaleGenerale localeGeneraleScript;
    private Gameplay       gameplayScript;
    private int            objectLength;

    private const string STYLE_RESOURCE_PATH = "Style/";

    // ─────────────────────────────────────────────────────────────────────
    //  AWAKE
    // ─────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(nomeCliente))
        {
            nomeCliente = gameObject.name;
            int p = nomeCliente.IndexOf('(');
            if (p > 0) nomeCliente = nomeCliente.Substring(0, p).TrimEnd();
        }

        localeGeneraleScript = GameObject.Find("# Script").GetComponent<LocaleGenerale>();
        gameplayScript       = GameObject.Find("# Script").GetComponent<Gameplay>();

        var longHairList     = new List<GameObject>();
        var shortHairList    = new List<GameObject>();
        var sopraccigliaList = new List<GameObject>();

        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>(true))
        {
            GameObject go = mr.gameObject;
            if (go.CompareTag(tagCapelliLunghi))
            {
                longHairList.Add(go);
                Color c = ReadMaterialColor(mr.sharedMaterial);
                HairState s = GetOrCreateState(go.name);
                s.colorBase    = c;
                s.colorCurrent = c;
            }
            else if (go.CompareTag(tagCapelliCorti))
            {
                shortHairList.Add(go);
                Color c = ReadMaterialColor(mr.sharedMaterial);
                HairState s = GetOrCreateState(go.name);
                s.colorBase    = c;
                s.colorCurrent = c;
            }
            else if (go.CompareTag(tagSopracciglia))
            {
                sopraccigliaList.Add(go);
            }
        }

        longHairList  = longHairList .OrderBy(o => o.name, new AlphanumComparatorFast()).ToList();
        shortHairList = shortHairList.OrderBy(o => o.name, new AlphanumComparatorFast()).ToList();

        meshObject.Clear();
        meshObject.AddRange(longHairList);
        meshObject.AddRange(shortHairList);

        sopracciglia = sopraccigliaList.ToArray();
        objectLength = meshObject.Count;

        punteggioTotaleInizio = objectLength * 4;
        punteggioTotale       = objectLength * 4;

        Debug.Log($"[{nomeCliente}] Awake | longHair={longHairList.Count}" +
                  $" shortHair={shortHairList.Count} sopracciglia={sopraccigliaList.Count}" +
                  $" | objectLength={objectLength}");

        if (hairStyle == null)
        {
            hairStyle = Resources.Load<HairStyleSO>(STYLE_RESOURCE_PATH + nomeCliente);
            if (hairStyle == null)
                Debug.LogWarning($"[{nomeCliente}] HairStyleSO non trovato. " +
                                 "Assegnalo nel prefab o crealo con SaveData().", this);
        }

        // COMBINED MESH: vera fusione in un blocco unico per materiale
        // Sostituisce StaticBatchingUtility.Combine
        BuildCombinedMeshes();

        // Registra con l'optimizer
        RegisterWithOptimizer();

    }

    private OculusQuestOptimizer FindOptimizer()
    {
        if (cachedOptimizer != null) return cachedOptimizer;

        GameObject centerEye = GameObject.Find("CenterEyeAnchor");
        if (centerEye != null)
            cachedOptimizer = centerEye.GetComponent<OculusQuestOptimizer>();

        if (cachedOptimizer == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                cachedOptimizer = mainCam.GetComponent<OculusQuestOptimizer>();
        }

        return cachedOptimizer;
    }


    private void RegisterWithOptimizer()
    {
        OculusQuestOptimizer optimizer = FindOptimizer();
        if (optimizer == null) return;

        // Trova il centro della testa per l'occlusione (tag dedicato "TestaOcclusione")
        Transform testaCliente = null;
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag("TestaOcclusione"))
            {
                testaCliente = child;
                break;
            }
        }

        if (testaCliente != null)
            optimizer.clientHeadCenter = testaCliente;

        Debug.Log($"[{nomeCliente}] Registrato con Optimizer, testa='{(testaCliente != null ? testaCliente.name : "non trovata")}'");
    }

    /// <summary>Registra le mesh combinate con l'optimizer per layer e occlusion.</summary>
    private void RegisterCombinedWithOptimizer()
    {
        OculusQuestOptimizer optimizer = FindOptimizer();
        if (optimizer == null) return;

        // Per ogni gruppo combinato, raccogli le posizioni world dei capelli
        var groupInfos = new List<OculusQuestOptimizer.CombinedGroupInfo>(combinedGroups.Count);

        foreach (CombinedGroup group in combinedGroups)
        {
            if (group.meshRenderer == null) continue;

            // Raccogli le posizioni world di ogni capello nel gruppo
            var positions = new List<Vector3>(group.hairIndices.Count);
            foreach (int idx in group.hairIndices)
            {
                if (meshObject[idx] != null)
                    positions.Add(meshObject[idx].transform.position);
            }

            groupInfos.Add(new OculusQuestOptimizer.CombinedGroupInfo
            {
                renderer       = group.meshRenderer,
                hairPositions  = positions.ToArray()
            });
        }

        optimizer.RegisterCombinedGroups(groupInfos);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  CONFRONTO E PAGAMENTO
    // ─────────────────────────────────────────────────────────────────────

    public IEnumerator Confronta(float inizioConfronto)
    {
        yield return new WaitForSeconds(inizioConfronto);

        if (hairStyle == null || hairStyle.entries.Count == 0)
        {
            Debug.LogError($"[{nomeCliente}] HairStyleSO mancante o vuoto — pagamento saltato.", this);
            PagaCliente(0);
            yield break;
        }

        int entryCount = Mathf.Min(objectLength, hairStyle.entries.Count);

        int basePunti = 0;
        for (int i = 0; i < entryCount; i++)
        {
            HairStyleSO.HairEntry e = hairStyle.entries[i];
            if (e.isRemoved)      basePunti += 4;
            if (!e.isRemoved)     { if (e.spray) basePunti++; if (e.oil) basePunti++; }
        }
        if (basePunti == 0) basePunti = entryCount;

        punteggioTotaleInizio = basePunti;
        punteggioTotale       = basePunti;

        int penalitaRimozioni = 0, penalitaMateriali = 0, penalitaSpray = 0, penalitaOlio = 0;

        for (int i = 0; i < entryCount; i++)
        {
            HairStyleSO.HairEntry entry = hairStyle.entries[i];

            if (entry.isRemoved)
            {
                if (meshObject[i] != null)
                {
                    punteggioTotale -= 4;
                    penalitaRimozioni += 4;
                }
            }
            else
            {
                if (meshObject[i] == null)
                {
                    punteggioTotale -= 4;
                    penalitaRimozioni += 4;
                }
                else
                {
                    MeshRenderer mr = meshObject[i].GetComponent<MeshRenderer>();
                    HairState    hs = GetOrCreateState(meshObject[i].name);

                    string materialeAtteso  = entry.material != null ? entry.material.name : "";
                    string materialeAttuale = mr != null && mr.sharedMaterial != null ? mr.sharedMaterial.name : "";

                    if (materialeAtteso != materialeAttuale) { punteggioTotale--; penalitaMateriali++; }
                    if (entry.spray != hs.spray)             { punteggioTotale--; penalitaSpray++;    }
                    if (entry.oil   != hs.oil)               { punteggioTotale--; penalitaOlio++;     }
                }
            }
        }

        if (DanniRicevuti > 0) punteggioTotale -= DanniRicevuti * 4;

        bool sopraccigliaTagliate = SonoStateTagliateSopracciglia();
        float percentuale = Mathf.Clamp01((float)punteggioTotale / punteggioTotaleInizio);

        Debug.Log($"[{nomeCliente}] CONFRONTA" +
                  $" | base={punteggioTotaleInizio} punti={punteggioTotale}" +
                  $" | penRim={penalitaRimozioni} penMat={penalitaMateriali}" +
                  $" | penSpray={penalitaSpray} penOlio={penalitaOlio}" +
                  $" | danni={DanniRicevuti} | {percentuale:P1}" +
                  (sopraccigliaTagliate ? " | SOPRACCIGLIA TAGLIATE" : ""));

        int pagaEffettiva = CalcolaPaga(percentuale, sopraccigliaTagliate);
        PagaCliente(pagaEffettiva);

        Debug.Log($"[{nomeCliente}] Qualita: {percentuale:P0} | Paga: ${pagaEffettiva}");
    }

    private bool SonoStateTagliateSopracciglia()
    {
        for (int i = 0; i < sopracciglia.Length; i++)
            if (sopracciglia[i] == null) return true;
        return false;
    }

    private int CalcolaPaga(float percentuale, bool sopraccigliaTagliate)
    {
        if (sopraccigliaTagliate)
        {
            if (localeGeneraleScript.Qualita > 0) localeGeneraleScript.Qualita--;
            return 0;
        }
        if (percentuale < 0.30f)
        {
            if (localeGeneraleScript.Qualita > 0) localeGeneraleScript.Qualita--;
            return 0;
        }
        if (percentuale < 0.60f) return pagaBaseCliente / 2;
        if (percentuale < 0.91f) return Mathf.RoundToInt(pagaBaseCliente * 0.80f);

        if (localeGeneraleScript.Qualita < 100) localeGeneraleScript.Qualita++;
        return pagaBaseCliente;
    }

    private void PagaCliente(int pagaEffettiva)
    {
        localeGeneraleScript.Progresso = Mathf.Clamp(localeGeneraleScript.Progresso + 1, 0, 100);
        gameplayScript.dayCash += pagaEffettiva;
        localeGeneraleScript.SoldiTotali += pagaEffettiva;
        ParticelleTestoSoldi.text = pagaEffettiva + " $";
        Animazione.SetBool("start", true);
        localeGeneraleScript.SuonoAcquistoCassa();
        localeGeneraleScript.UpdateCassa();
    }

    // ─────────────────────────────────────────────────────────────────────
    //  LETTURA E SCRITTURA STILE
    // ─────────────────────────────────────────────────────────────────────

    [ContextMenu("Read/Apply Style")]
    public void ReadData()
    {
        if (hairStyle == null)
        {
            Debug.LogWarning("[HairManager] Nessun HairStyleSO assegnato.", this);
            return;
        }

        int count = Mathf.Min(meshObject.Count, hairStyle.entries.Count);
        for (int i = 0; i < count; i++)
        {
            if (meshObject[i] == null) continue;
            HairStyleSO.HairEntry entry = hairStyle.entries[i];

            if (entry.isRemoved)
            {
                if (Application.isPlaying) Destroy(meshObject[i]);
                else                       DestroyImmediate(meshObject[i]);
                meshObject[i] = null;
                continue;
            }

            if (entry.material != null)
            {
                MeshRenderer mr = meshObject[i].GetComponent<MeshRenderer>();
                mr.sharedMaterial = entry.material;
            }

            HairState hs = GetOrCreateState(meshObject[i].name);
            hs.spray        = entry.spray;
            hs.oil          = entry.oil;
            hs.colorCurrent = entry.material != null
                ? ReadMaterialColor(entry.material)
                : hs.colorBase;
        }
        // Dopo ReadData, rebuild tutte le mesh combinate
        foreach (CombinedGroup g in combinedGroups)
            g.dirty = true;
    }

    [ContextMenu("Save Hair Style")]
    public void SaveData()
    {
#if UNITY_EDITOR
        string soPath = "Assets/Resources/Style/" + nomeCliente + ".asset";

        HairStyleSO so    = AssetDatabase.LoadAssetAtPath<HairStyleSO>(soPath);
        bool        isNew = so == null;
        if (isNew) so = ScriptableObject.CreateInstance<HairStyleSO>();

        Undo.RecordObject(so, "Save Hair Style");
        so.entries.Clear();

        for (int i = 0; i < objectLength; i++)
        {
            var entry = new HairStyleSO.HairEntry();

            if (meshObject[i] == null)
            {
                entry.isRemoved = true;
            }
            else
            {
                entry.isRemoved = false;
                entry.material  = TrovaMaterialeCorrispondente(
                                      meshObject[i].GetComponent<Renderer>().sharedMaterial);
                HairState hs = GetOrCreateState(meshObject[i].name);
                entry.spray  = hs.spray;
                entry.oil    = hs.oil;
            }

            so.entries.Add(entry);
        }

        if (isNew) AssetDatabase.CreateAsset(so, soPath);
        else        EditorUtility.SetDirty(so);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        hairStyle = AssetDatabase.LoadAssetAtPath<HairStyleSO>(soPath);
        Debug.Log($"[{nomeCliente}] Stile salvato: {soPath} ({so.entries.Count} entries)", this);
#else
        Debug.LogWarning("[HairManager] SaveData() e disponibile solo nell'Editor Unity.");
#endif
    }

    private Material TrovaMaterialeCorrispondente(Material mat)
    {
        if (mat == null) return null;
        foreach (Material m in hairMaterials)
            if (m != null && m.name == mat.name) return m;
        return mat;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  SISTEMA FOTO (Editor Only)
    // ─────────────────────────────────────────────────────────────────────

    public void CreatePhoto()
    {
#if UNITY_EDITOR
        GameObject cloth = GameObject.Find("Cloth");
        if (cloth != null)
            cloth.GetComponent<SkinnedMeshRenderer>().enabled = false;

        Instantiate(SistemaPerFoto, SistemaPerFoto.transform.position, SistemaPerFoto.transform.rotation);

        GameObject tshirt = GameObject.Find("T-Shirt");
        if (tshirt != null)
            tshirt.GetComponent<MeshRenderer>().material.color =
                ColoriMagliette[Random.Range(0, ColoriMagliette.Length)];

        SetupOcchiStaticiPerFoto("TestaCliente_1", "OcchioL_1");
        SetupOcchiStaticiPerFoto("TestaCliente_2", "OcchioL_2");
        SetupOcchiStaticiPerFoto("TestaCliente_3", "OcchioL_3");

        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        var recorderController = new RecorderController(controllerSettings);

        var imageRecorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();
        imageRecorder.name          = "Image";
        imageRecorder.Enabled       = true;
        imageRecorder.OutputFormat  = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
        imageRecorder.CaptureAlpha  = false;
        imageRecorder.OutputFile    = System.IO.Path.Combine(@"C:\Users\devda\Desktop\", nomeCliente + ".png");
        imageRecorder.imageInputSettings = new GameViewInputSettings
        {
            OutputWidth  = 2048,
            OutputHeight = 2048,
        };

        controllerSettings.AddRecorderSettings(imageRecorder);
        controllerSettings.SetRecordModeToFrameInterval(0, 0);
        recorderController.PrepareRecording();
        recorderController.StartRecording();

        StartCoroutine(RestoreCreatePhoto());
#endif
    }

    private void SetupOcchiStaticiPerFoto(string testaName, string occhioName)
    {
        GameObject testa = GameObject.Find(testaName);
        if (testa == null) return;

        OcchiCliente occhi = testa.GetComponentInChildren<OcchiCliente>();
        if (occhi != null) occhi.enabled = false;

        GameObject occhio = GameObject.Find(occhioName);
        if (occhio == null) return;

        Material   eyeMat   = occhio.GetComponent<MeshRenderer>().material;
        GameObject occhioLS = GameObject.Find("OcchioLS");
        GameObject occhioRS = GameObject.Find("OcchioRS");
        if (occhioLS != null) occhioLS.GetComponent<MeshRenderer>().material = eyeMat;
        if (occhioRS != null) occhioRS.GetComponent<MeshRenderer>().material = eyeMat;
    }

    private IEnumerator RestoreCreatePhoto()
    {
        yield return new WaitForSeconds(1f);

        GameObject clone = GameObject.Find("SistemaPerFoto(Clone)");
        if (clone != null) Destroy(clone);

        GameObject cloth = GameObject.Find("Cloth");
        if (cloth != null) cloth.GetComponent<SkinnedMeshRenderer>().enabled = true;

        OcchiCliente occhi = GetComponentInChildren<OcchiCliente>();
        if (occhi != null) occhi.enabled = true;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  HSBColor
    // ─────────────────────────────────────────────────────────────────────

    private struct HSBColor
    {
        public float h, s, b, a;

        public HSBColor(float h, float s, float b, float a) { this.h = h; this.s = s; this.b = b; this.a = a; }

        public static HSBColor FromColor(Color color)
        {
            HSBColor ret = new HSBColor(0f, 0f, 0f, color.a);
            float r = color.r, g = color.g, bVal = color.b;
            float max = Mathf.Max(r, Mathf.Max(g, bVal));
            if (max <= 0f) return ret;

            float min = Mathf.Min(r, Mathf.Min(g, bVal));
            float dif = max - min;

            if (max > min)
            {
                if      (g    == max) ret.h = (bVal - r) / dif * 60f + 120f;
                else if (bVal == max) ret.h = (r    - g) / dif * 60f + 240f;
                else if (bVal >    g) ret.h = (g - bVal) / dif * 60f + 360f;
                else                  ret.h = (g - bVal) / dif * 60f;

                if (ret.h < 0f) ret.h += 360f;
            }

            ret.h *= 1f / 360f;
            ret.s  = dif / max;
            ret.b  = max;
            return ret;
        }

        public static Color ToColor(HSBColor c)
        {
            float r = c.b, g = c.b, bVal = c.b;
            if (c.s != 0f)
            {
                float max = c.b, dif = c.b * c.s, min = c.b - dif, h = c.h * 360f;
                if      (h <  60f) { r = max; g = h * dif / 60f + min; bVal = min; }
                else if (h < 120f) { r = -(h - 120f) * dif / 60f + min; g = max; bVal = min; }
                else if (h < 180f) { r = min; g = max; bVal = (h - 120f) * dif / 60f + min; }
                else if (h < 240f) { r = min; g = -(h - 240f) * dif / 60f + min; bVal = max; }
                else if (h < 300f) { r = (h - 240f) * dif / 60f + min; g = min; bVal = max; }
                else if (h <= 360f){ r = max; g = min; bVal = -(h - 360f) * dif / 60f + min; }
                else               { r = 0f; g = 0f; bVal = 0f; }
            }
            return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(bVal), c.a);
        }

        public static HSBColor Lerp(HSBColor a, HSBColor b, float t)
        {
            float h, s;
            if      (a.b == 0f) { h = b.h; s = b.s; }
            else if (b.b == 0f) { h = a.h; s = a.s; }
            else
            {
                if      (a.s == 0f) h = b.h;
                else if (b.s == 0f) h = a.h;
                else
                {
                    float angle = Mathf.LerpAngle(a.h * 360f, b.h * 360f, t);
                    while (angle <    0f) angle += 360f;
                    while (angle > 360f) angle -= 360f;
                    h = angle / 360f;
                }
                s = Mathf.Lerp(a.s, b.s, t);
            }
            return new HSBColor(h, s, Mathf.Lerp(a.b, b.b, t), Mathf.Lerp(a.a, b.a, t));
        }
    }

    private static Color ReadMaterialColor(Material mat)
    {
        if (mat == null) return Color.white;
        if (mat.HasProperty("_BaseColor")) return mat.GetColor("_BaseColor");
        return mat.color;
    }

    private static Color LerpViaHSB(Color a, Color b, float t)
        => HSBColor.ToColor(HSBColor.Lerp(HSBColor.FromColor(a), HSBColor.FromColor(b), t));
}

// ─────────────────────────────────────────────────────────────────────────────
//  COMPARATORE ALFANUMERICO
// ─────────────────────────────────────────────────────────────────────────────

public class AlphanumComparatorFast : IComparer<string>
{
    public int Compare(string x, string y)
    {
        if (x == null) return 0;
        if (y == null) return 0;

        int len1 = x.Length, len2 = y.Length;
        int marker1 = 0,     marker2 = 0;

        while (marker1 < len1 && marker2 < len2)
        {
            char ch1 = x[marker1], ch2 = y[marker2];
            char[] space1 = new char[len1]; int loc1 = 0;
            char[] space2 = new char[len2]; int loc2 = 0;

            do { space1[loc1++] = ch1; marker1++;
                 if (marker1 < len1) ch1 = x[marker1]; else break;
            } while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

            do { space2[loc2++] = ch2; marker2++;
                 if (marker2 < len2) ch2 = y[marker2]; else break;
            } while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

            string str1 = new string(space1, 0, loc1);
            string str2 = new string(space2, 0, loc2);

            int result = char.IsDigit(space1[0]) && char.IsDigit(space2[0])
                ? int.Parse(str1).CompareTo(int.Parse(str2))
                : string.Compare(str1, str2, System.StringComparison.Ordinal);

            if (result != 0) return result;
        }

        return len1 - len2;
    }
}
