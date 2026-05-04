#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Migra automaticamente tutti gli EventSystem nelle scene del progetto
/// da StandaloneInputModule a InputSystemUIInputModule.
///
/// Gira automaticamente all'avvio dell'Editor (InitializeOnLoadMethod)
/// e tramite menu: Virtual Barbershop > Fix EventSystem (New Input System)
/// </summary>
[InitializeOnLoad]
public static class InputSystemMigration
{
    private static readonly string[] SCENE_FOLDERS = { "Assets/# Progetto/Scenes" };

    // Gira automaticamente quando l'Editor carica il dominio
    static InputSystemMigration()
    {
        // Aspetta che l'Editor sia completamente inizializzato prima di fare modifiche
        EditorApplication.delayCall += AutoFix;
    }

    private static void AutoFix()
    {
        // Controlla solo senza aprire scene — lavora sulle scene già aperte
        bool anyFixed = false;

        for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            bool changed = false;
            foreach (var root in scene.GetRootGameObjects())
            {
                int n = FixRecursive(root);
                if (n > 0) { changed = true; anyFixed = true; }
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                Debug.Log($"[InputSystemMigration] Auto-fixed EventSystem in scene: {scene.name}");
            }
        }

        if (anyFixed)
            Debug.Log("[InputSystemMigration] Salvare le scene modificate con File > Save.");
    }

    // ─── MENU ITEM ────────────────────────────────────────────────────────

    [MenuItem("Virtual Barbershop/Fix EventSystem (New Input System)")]
    public static void FixAllScenes()
    {
        string currentPath = UnityEngine.SceneManagement.SceneManager
                                .GetActiveScene().path;

        string[] guids      = AssetDatabase.FindAssets("t:Scene", SCENE_FOLDERS);
        int      sceneFixed = 0;
        int      compFixed  = 0;

        foreach (string guid in guids)
        {
            string path  = AssetDatabase.GUIDToAssetPath(guid);
            var    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            bool changed = false;
            foreach (var root in scene.GetRootGameObjects())
            {
                int n = FixRecursive(root);
                if (n > 0) { compFixed += n; changed = true; }
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                sceneFixed++;
            }

            if (scene.path != currentPath)
                EditorSceneManager.CloseScene(scene, true);
        }

        string msg = $"{sceneFixed} scene fixate, {compFixed} StandaloneInputModule → InputSystemUIInputModule";
        Debug.Log($"[InputSystemMigration] {msg}");
        EditorUtility.DisplayDialog("Fix EventSystem completato", msg, "OK");
    }

    // ─── HELPER ──────────────────────────────────────────────────────────

    private static int FixRecursive(GameObject go)
    {
        int count  = 0;
        var module = go.GetComponent<StandaloneInputModule>();

        if (module != null)
        {
            Object.DestroyImmediate(module, true);
            if (go.GetComponent<InputSystemUIInputModule>() == null)
                go.AddComponent<InputSystemUIInputModule>();
            count++;
        }

        foreach (Transform child in go.transform)
            count += FixRecursive(child.gameObject);

        return count;
    }
}
#endif
