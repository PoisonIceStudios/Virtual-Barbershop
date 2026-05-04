using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Menu: Virtual Barbershop > Migra Stili txt -> SO
///
/// Scansiona Assets/Resources/Style/ alla ricerca di file .txt, e per ognuno
/// crea (o sovrascrive) il corrispondente HairStyleSO .asset.
///
/// Formato atteso del txt (una riga per capello, in ordine):
///   null                              -> capello da rimuovere (isRemoved = true)
///   NomeCapello|NomeMateriale|spray|oil  -> capello da tenere  (isRemoved = false)
///
/// I valori spray e oil sono "True" o "False" (case-insensitive).
/// </summary>
public static class HairStyleMigrationTool
{
    private const string STYLE_FOLDER   = "Assets/Resources/Style/";
    private const string MENU_ITEM_PATH = "Virtual Barbershop/Migra Stili txt -> SO";

    [MenuItem(MENU_ITEM_PATH)]
    public static void MigraTutti()
    {
        string[] txtFiles = Directory.GetFiles(STYLE_FOLDER, "*.txt");

        if (txtFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("Migrazione", "Nessun file .txt trovato in:\n" + STYLE_FOLDER, "OK");
            return;
        }

        int successi = 0, falliti = 0;
        string riepilogo = "";

        foreach (string txtPath in txtFiles)
        {
            string nome = Path.GetFileNameWithoutExtension(txtPath);
            string soPath = STYLE_FOLDER + nome + ".asset";

            try
            {
                (int tenuti, int rimossi, int errori) = MigraFile(txtPath, soPath);
                riepilogo += $"{nome}: {tenuti} da tenere, {rimossi} da rimuovere\n";
                if (errori > 0) riepilogo += $"  ATTENZIONE: {errori} righe malformate\n";
                successi++;
            }
            catch (Exception ex)
            {
                riepilogo += $"{nome}: ERRORE — {ex.Message}\n";
                falliti++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Migrazione completata",
            $"File elaborati: {successi} / {txtFiles.Length}\n" +
            (falliti > 0 ? $"Falliti: {falliti}\n" : "") +
            "\n" + riepilogo,
            "OK");

        Debug.Log("[HairStyleMigrationTool] Migrazione completata:\n" + riepilogo);
    }

    // ─────────────────────────────────────────────────────────────────────────

    private static (int tenuti, int rimossi, int errori) MigraFile(string txtPath, string soPath)
    {
        string[] lines = File.ReadAllLines(txtPath);

        HairStyleSO so    = AssetDatabase.LoadAssetAtPath<HairStyleSO>(soPath);
        bool        isNew = so == null;
        if (isNew) so = ScriptableObject.CreateInstance<HairStyleSO>();

        Undo.RecordObject(so, "Migra Stile da txt");
        so.entries.Clear();

        int tenuti = 0, rimossi = 0, errori = 0;

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
                    Debug.LogWarning($"[Migrazione] Riga malformata in {Path.GetFileName(txtPath)}: {trimmed}");
                    errori++;
                    continue;
                }

                entry.isRemoved = false;
                entry.material  = TrovaMaterialePerNome(parts[1].Trim());
                entry.spray     = string.Equals(parts[2].Trim(), "True", StringComparison.OrdinalIgnoreCase);
                entry.oil       = string.Equals(parts[3].Trim(), "True", StringComparison.OrdinalIgnoreCase);
                tenuti++;
            }

            so.entries.Add(entry);
        }

        if (isNew) AssetDatabase.CreateAsset(so, soPath);
        else        EditorUtility.SetDirty(so);

        return (tenuti, rimossi, errori);
    }

    // Cerca il materiale per nome in tutti gli asset del progetto.
    private static Material TrovaMaterialePerNome(string nome)
    {
        string[] guids = AssetDatabase.FindAssets($"t:Material {nome}");
        foreach (string guid in guids)
        {
            string   path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat  = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && mat.name == nome) return mat;
        }

        Debug.LogWarning($"[Migrazione] Materiale '{nome}' non trovato nel progetto.");
        return null;
    }
}
