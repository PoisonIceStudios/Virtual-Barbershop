using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VirtualBarbershop.Editor
{
    /// <summary>
    /// Rimuove componenti con script mancante (null-script E type-missing via GUID)
    /// da scene aperte, tutte le scene del progetto e tutti i prefab.
    ///
    /// Usa due metodi combinati:
    ///  1. GameObjectUtility.RemoveMonoBehavioursWithMissingScript()  → null-script reference
    ///  2. SerializedObject scan su m_Component                       → type-missing (GUID valido, classe inesistente)
    /// </summary>
    public static class RemoveMissingScripts
    {
        // ── Menu items ─────────────────────────────────────────────────────────

        [MenuItem("Virtual Barbershop/Rimuovi Script Mancanti/Dalla scena attiva")]
        public static void RemoveFromActiveScene()
        {
            int removed = CleanScene(SceneManager.GetActiveScene());
            if (removed > 0)
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[RemoveMissingScripts] Scena '{SceneManager.GetActiveScene().name}': rimossi {removed} script mancanti.");
            EditorUtility.DisplayDialog("Script mancanti rimossi",
                $"Rimossi {removed} componenti con script mancante dalla scena attiva.", "OK");
        }

        [MenuItem("Virtual Barbershop/Rimuovi Script Mancanti/Da tutte le scene aperte")]
        public static void RemoveFromAllOpenScenes()
        {
            int total = 0;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                int removed = CleanScene(scene);
                if (removed > 0)
                    EditorSceneManager.MarkSceneDirty(scene);
                total += removed;
                Debug.Log($"[RemoveMissingScripts] Scena '{scene.name}': rimossi {removed} script mancanti.");
            }
            EditorUtility.DisplayDialog("Script mancanti rimossi",
                $"Rimossi {total} componenti da tutte le scene aperte.", "OK");
        }

        [MenuItem("Virtual Barbershop/Rimuovi Script Mancanti/Da TUTTE le scene del progetto")]
        public static void RemoveFromAllProjectScenes()
        {
            // Salva le scene attualmente aperte per poterle ripristinare
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string[] scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            int totalScenes = 0;
            int totalRemoved = 0;

            try
            {
                for (int i = 0; i < scenePaths.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(scenePaths[i]);
                    EditorUtility.DisplayProgressBar("Pulizia scene...",
                        $"Scena {i + 1}/{scenePaths.Length}: {path}",
                        (float)i / scenePaths.Length);

                    // Carica la scena in modalità additiva per non perdere quella aperta
                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    if (!scene.isLoaded) continue;

                    int removed = CleanScene(scene);
                    if (removed > 0)
                    {
                        EditorSceneManager.SaveScene(scene);
                        totalScenes++;
                        totalRemoved += removed;
                        Debug.Log($"[RemoveMissingScripts] Scena '{path}': rimossi {removed} script mancanti.");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog("Scene del progetto pulite",
                $"Rimossi {totalRemoved} componenti da {totalScenes} scene.", "OK");
        }

        [MenuItem("Virtual Barbershop/Rimuovi Script Mancanti/Da tutti i prefab del progetto")]
        public static void RemoveFromAllPrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            int totalObjects = 0;
            int totalRemoved = 0;

            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    EditorUtility.DisplayProgressBar("Pulizia prefab...",
                        $"Prefab {i + 1}/{guids.Length}: {path}",
                        (float)i / guids.Length);

                    using (var editScope = new PrefabUtility.EditPrefabContentsScope(path))
                    {
                        int removed = CleanGameObjectRecursive(editScope.prefabContentsRoot);
                        if (removed > 0)
                        {
                            totalObjects++;
                            totalRemoved += removed;
                            Debug.Log($"[RemoveMissingScripts] Prefab '{path}': rimossi {removed} script mancanti.");
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
            }

            EditorUtility.DisplayDialog("Prefab puliti",
                $"Rimossi {totalRemoved} componenti da {totalObjects} prefab.", "OK");
        }

        [MenuItem("Virtual Barbershop/Rimuovi Script Mancanti/DIAGNOSTICA - Trova script mancanti (solo log)")]
        public static void DiagnoseMissingScripts()
        {
            int found = 0;

            // Scansiona scene aperte
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (GameObject root in scene.GetRootGameObjects())
                    found += DiagnoseGameObjectRecursive(root, scene.name);
            }

            // Scansiona prefab
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                    found += DiagnoseGameObjectRecursive(prefab, path);
            }

            string msg = found > 0
                ? $"Trovati {found} componenti con script mancante.\nControlla la Console per i dettagli."
                : "Nessun script mancante trovato nelle scene aperte e nei prefab caricati.";

            Debug.Log($"[RemoveMissingScripts] DIAGNOSI COMPLETATA: {found} componenti trovati.");
            EditorUtility.DisplayDialog("Diagnosi script mancanti", msg, "OK");
        }

        // ── Core clean logic ───────────────────────────────────────────────────

        private static int CleanScene(Scene scene)
        {
            int total = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
                total += CleanGameObjectRecursive(root);
            return total;
        }

        /// <summary>
        /// Rimozione in due passaggi:
        ///  1. GameObjectUtility (null-script)
        ///  2. SerializedObject scan (type-missing: GUID esiste, classe no)
        /// </summary>
        private static int CleanGameObjectRecursive(GameObject go)
        {
            if (go == null) return 0;

            int removed = 0;

            // Prima i figli (bottom-up)
            foreach (Transform child in go.transform)
                removed += CleanGameObjectRecursive(child.gameObject);

            // --- Passaggio 1: null-script ---
            int before = go.GetComponents<Component>().Length;
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            int after = go.GetComponents<Component>().Length;
            removed += before - after;

            // --- Passaggio 2: type-missing via SerializedObject ---
            removed += RemoveTypeMissingComponents(go);

            return removed;
        }

        /// <summary>
        /// Scansiona l'array m_Component via SerializedObject e rimuove le voci dove
        /// il riferimento al component è nullo (GUID nel file ma classe non trovata).
        /// </summary>
        private static int RemoveTypeMissingComponents(GameObject go)
        {
            // Conta i component che GetComponents vede come null (script mancante ma non rimosso dal passo 1)
            Component[] components = go.GetComponents<Component>();
            bool hasMissing = false;
            foreach (Component c in components)
            {
                if (c == null)
                {
                    hasMissing = true;
                    break;
                }
            }
            if (!hasMissing) return 0;

            // Usa SerializedObject per accedere direttamente all'array interno dei component
            var serializedGO = new SerializedObject(go);
            SerializedProperty compArray = serializedGO.FindProperty("m_Component");
            if (compArray == null) return 0;

            int removed = 0;
            for (int i = compArray.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty element = compArray.GetArrayElementAtIndex(i);
                SerializedProperty compRef = element.FindPropertyRelative("component");
                if (compRef != null && compRef.objectReferenceValue == null)
                {
                    compArray.DeleteArrayElementAtIndex(i);
                    removed++;
                }
            }

            if (removed > 0)
                serializedGO.ApplyModifiedProperties();

            return removed;
        }

        // ── Diagnosi (solo log, non modifica) ──────────────────────────────────

        private static int DiagnoseGameObjectRecursive(GameObject go, string context)
        {
            if (go == null) return 0;
            int found = 0;

            foreach (Transform child in go.transform)
                found += DiagnoseGameObjectRecursive(child.gameObject, context);

            // Metodo 1: null entries
            Component[] components = go.GetComponents<Component>();
            foreach (Component c in components)
            {
                if (c == null)
                {
                    Debug.LogWarning($"[MissingScript] Tipo: NULL  |  GameObject: '{go.name}'  |  Contesto: '{context}'", go);
                    found++;
                }
            }

            // Metodo 2: SerializedObject
            var so = new SerializedObject(go);
            SerializedProperty compArray = so.FindProperty("m_Component");
            if (compArray != null)
            {
                for (int i = 0; i < compArray.arraySize; i++)
                {
                    SerializedProperty compRef = compArray.GetArrayElementAtIndex(i)
                        .FindPropertyRelative("component");
                    if (compRef != null && compRef.objectReferenceValue == null &&
                        compRef.objectReferenceInstanceIDValue != 0)
                    {
                        Debug.LogWarning($"[MissingScript] Tipo: TYPE-MISSING (GUID presente)  |  GameObject: '{go.name}'  |  Contesto: '{context}'", go);
                        found++;
                    }
                }
            }

            return found;
        }
    }
}
