using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscludiPlayerController : MonoBehaviour
{

    Collider PlayerController;
    Collider[] ListaColliders;

    void Awake()
    {

        PlayerController = GameObject.FindFirstObjectByType<CharacterController>();

        ListaColliders = this.GetComponentsInChildren<Collider>();

        if (ListaColliders != null)
        {
            foreach (var col in ListaColliders)
            {
               if (col && col.enabled)
               {
                       Physics.IgnoreCollision(PlayerController, col, true);
               }
            }
        }

    }

}
