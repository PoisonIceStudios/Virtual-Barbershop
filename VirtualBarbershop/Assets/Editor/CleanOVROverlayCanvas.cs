#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Rimuove componenti OVROverlayCanvas_TMPChanged orfani (senza Canvas assegnato)
/// che causano NullReferenceException
/// </summary>
public static class CleanOVROverlayCanvas
{
    [MenuItem("Virtual Barbershop/Clean OVROverlayCanvas (rimuovi componenti orfani)")]
    public static void RemoveOrphanedOVRComponents()
    {
        int removed = 0;

        // Cerca tutti i GameObject in tutte le scene caricate
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            foreach (var root in scene.GetRootGameObjects())
            {
                removed += RemoveOrphanedRecursive(root);
            }
        }

        Debug.Log($"[CleanOVROverlayCanvas] Rimossi {removed} componenti OVROverlayCanvas_TMPChanged orfani.");
        if (removed > 0) EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    private static int RemoveOrphanedRecursive(GameObject go)
    {
        int count = 0;

        // Tenta di ottenere il componente (se esiste)
        var components = go.GetComponents<Component>();
        foreach (var comp in components)
        {
            // Verifica se il tipo è OVROverlayCanvas_TMPChanged (per nome, poiché potrebbe non esistere se assente dalla build)
            if (comp != null && comp.GetType().Name == "OVROverlayCanvas_TMPChanged")
            {
                // Verifica se TargetCanvas è assegnato
                var targetCanvasField = comp.GetType().GetField("TargetCanvas");
                if (targetCanvasField != null)
                {
                    var targetCanvas = targetCanvasField.GetValue(comp) as Canvas;
                    if (targetCanvas == null)
                    {
                        // È orfano — rimuovilo
                        Object.DestroyImmediate(comp);
                        count++;
                        Debug.Log($"[CleanOVROverlayCanvas] Rimosso componente orfano da: {go.name}");
                    }
                }
            }
        }

        // Ricerca ricorsiva nei figli
        foreach (Transform child in go.transform)
        {
            count += RemoveOrphanedRecursive(child.gameObject);
        }

        return count;
    }
}
#endif
