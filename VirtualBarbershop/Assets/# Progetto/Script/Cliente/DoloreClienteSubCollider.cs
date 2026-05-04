using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoloreClienteSubCollider : MonoBehaviour
{

    public bool IsTrigger;
    DoloreCliente DoloreClienteScript;

    private void Start()
    {
        if (!IsTrigger) { 
          DoloreClienteScript = this.gameObject.GetComponentInParent<DoloreCliente>();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("LamaRasoio") && !IsTrigger)
        {
            if (collision.relativeVelocity.magnitude > 0.6f)
                DoloreClienteScript.Dolore();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsTrigger || !other.CompareTag("LamaRasoio")) return;

        // Feedback visivo: evidenzia il rasoio in rosso quando entra nella zona di pericolo
        Transform lama = other.gameObject.transform.parent?.parent;
        if (lama != null && lama.TryGetComponent<Renderer>(out var r))
            r.material.color = Color.red;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsTrigger || !other.CompareTag("LamaRasoio")) return;

        Transform lama = other.gameObject.transform.parent?.parent;
        if (lama != null && lama.TryGetComponent<Renderer>(out var r))
            r.material.color = Color.green;
    }

}
