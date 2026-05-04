using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Inspector per HairManager.
/// Aggiunge pulsanti per salvare/leggere lo stile e scattare la foto del cliente.
/// </summary>
[CustomEditor(typeof(HairManager))]
public class EditorScript : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HairManager script = (HairManager)target;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("── Stile ──────────────────────", EditorStyles.boldLabel);

        if (GUILayout.Button("Salva Stile Corrente (Create HairStyle)"))
        {
            script.SaveData();
            script.CreatePhoto();
        }

        if (GUILayout.Button("Applica Stile al Cliente (Read HairStyle)"))
        {
            script.ReadData();
        }

        if (GUILayout.Button("Scatta Foto Cliente"))
        {
            script.CreatePhoto();
        }

        if (GUILayout.Button("Esegui Confronta (test)"))
        {
            script.StartCoroutine(script.Confronta(0));
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("── Migrazione txt -> SO ────────", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Legge il file .txt in Assets/Resources/Style/{nome} e crea un HairStyleSO corretto.\n" +
            "Le righe 'null' nel txt diventano isRemoved=true nel SO.\n" +
            "Richiede che hairMaterials sia popolato nell'Inspector.",
            MessageType.Info);

        if (GUILayout.Button("Migra txt -> SO per questo cliente"))
        {
            MigraStileDaTxt(script);
        }
    }

    private void MigraStileDaTxt(HairManager script)
    {
        // Deriva il nome del cliente dal nome del GameObject
        string nome = script.gameObject.name;
        int p = nome.IndexOf('(');
        if (p > 0) nome = nome.Substring(0, p).TrimEnd();

        // Carica il file txt
        string txtPath = "Assets/Resources/Style/" + nome + ".txt";
        if (!File.Exists(txtPath))
        {
            EditorUtility.DisplayDialog("Errore", "File non trovato:\n" + txtPath, "OK");
            return;
        }

        string[] lines = File.ReadAllLines(txtPath);

        // Crea o carica il SO
        string soPath = "Assets/Resources/Style/" + nome + ".asset";
        HairStyleSO so = AssetDatabase.LoadAssetAtPath<HairStyleSO>(soPath);
        bool isNew = so == null;
        if (isNew) so = ScriptableObject.CreateInstance<HairStyleSO>();

        Undo.RecordObject(so, "Migra Stile da txt");
        so.entries.Clear();

        int rimossi = 0, tenuti = 0, errori = 0;

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            var entry = new HairStyleSO.HairEntry();

            if (trimmed == "null")
            {
                entry.isRemoved = true;
                rimossi++;
            }
            else
            {
                string[] parts = trimmed.Split('|');
                if (parts.Length < 4)
                {
                    Debug.LogWarning("[Migrazione] Riga malformata: " + trimmed);
                    errori++;
                    continue;
                }

                entry.isRemoved = false;
                entry.material  = TrovaMaterialePerNome(script, parts[1].Trim());
                entry.spray     = string.Equals(parts[2].Trim(), "True", StringComparison.OrdinalIgnoreCase);
                entry.oil       = string.Equals(parts[3].Trim(), "True", StringComparison.OrdinalIgnoreCase);
                tenuti++;
            }

            so.entries.Add(entry);
        }

        if (isNew) AssetDatabase.CreateAsset(so, soPath);
        else        EditorUtility.SetDirty(so);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Assegna il SO allo script se non gia assegnato
        HairStyleSO soCaricato = AssetDatabase.LoadAssetAtPath<HairStyleSO>(soPath);
        if (script.hairStyle != soCaricato)
        {
            Undo.RecordObject(script, "Assegna HairStyle");
            script.hairStyle = soCaricato;
            EditorUtility.SetDirty(script);
        }

        EditorUtility.DisplayDialog(
            "Migrazione completata",
            $"Cliente: {nome}\n" +
            $"Voci totali: {so.entries.Count}\n" +
            $"  - Da tenere: {tenuti}\n" +
            $"  - Da rimuovere (isRemoved=true): {rimossi}\n" +
            $"  - Righe con errori: {errori}\n\n" +
            $"SO salvato in: {soPath}",
            "OK");

        Debug.Log($"[Migrazione] {nome}: {tenuti} da tenere, {rimossi} da rimuovere. SO: {soPath}");
    }

    private static Material TrovaMaterialePerNome(HairManager script, string nome)
    {
        // Prima cerca in hairMaterials dell'Inspector
        if (script.hairMaterials != null)
        {
            foreach (Material m in script.hairMaterials)
                if (m != null && m.name == nome) return m;
        }

        // Fallback: cerca in tutti gli asset del progetto
        string[] guids = AssetDatabase.FindAssets($"t:Material {nome}");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && mat.name == nome) return mat;
        }

        Debug.LogWarning($"[Migrazione] Materiale '{nome}' non trovato.");
        return null;
    }
}
