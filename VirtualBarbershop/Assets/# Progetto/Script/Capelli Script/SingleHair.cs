// SingleHair e stato rimosso.
// Sostituito da:
//   HairData.cs     - dati per capello (Spray, Oil, Schiuma, colore)
//   LamaTrigger.cs  - logica taglio sui blade collider dei tool
//   Particelle_Hit.cs - logica spray/olio/schiuma/colore sulle particelle
//
// Azioni richieste in Unity:
//   1. Su ogni prefab capello: rimuovi SingleHair, aggiungi HairData
//      Riassegna i campi MeshSchiuma e MaterialeSchiuma in HairData
//   2. Su Forbice: aggiungi LamaTrigger a ciascun ColliderTaglio[0-3]
//   3. Su Rasoio: aggiungi LamaTrigger al GameObject del ColliderLama
//   4. Su RasoioElettrico: aggiungi LamaTrigger al GameObject del ColliderLama
//   5. Sui prefab particelle spray: aggiungi un Collider trigger (es. SphereCollider Is Trigger)
//      per permettere a Particelle_Hit.OnTriggerEnter di rilevare i capelli
