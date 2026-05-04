using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject che rappresenta lo stile di acconciatura obiettivo per un cliente.
/// Creato in Play Mode (Editor) tramite il tasto destro su HairManager → "Save Hair Style",
/// oppure premendo il pulsante SaveData() collegato a un Button nella scena.
///
/// Workflow:
///   1. Entra in Play Mode
///   2. Rimuovi i capelli, applica colori, spray e olio come vuoi
///   3. Chiama SaveData() su HairManager (tasto destro sul componente o Button UI)
///   4. Esci da Play Mode — il file .asset viene salvato in Assets/Resources/Style/
///   5. Trascina il file .asset nel campo "Hair Style" del prefab cliente
/// </summary>
[CreateAssetMenu(fileName = "HairStyle_Cliente", menuName = "Virtual Barbershop/Hair Style")]
public class HairStyleSO : ScriptableObject
{
    [System.Serializable]
    public class HairEntry
    {
        [Tooltip("True se questo capello deve essere RIMOSSO nel taglio finale (era 'null' nel vecchio .txt)")]
        public bool isRemoved;

        [Tooltip("Materiale atteso su questo capello. Ignorato se isRemoved = true.")]
        public Material material;

        [Tooltip("Il capello deve essere stato trattato con Spray")]
        public bool spray;

        [Tooltip("Il capello deve essere stato trattato con Olio")]
        public bool oil;
    }

    [Tooltip("Lista indicizzata di tutti i capelli (LongHair prima, ShortHair dopo), in ordine alfanumerico. " +
             "L'indice corrisponde all'indice nell'array meshObject di HairManager.")]
    public List<HairEntry> entries = new();
}
