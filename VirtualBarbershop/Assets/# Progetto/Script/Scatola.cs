 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scatola : MonoBehaviour
{
    /* Modifica */
    [Header("Zone Drag:")]
    public Transform DragObjectRight;
    public Transform DragObjectLeft;

    [Header("Box Doors:")]
    [SerializeField] GameObject antaRight;
    [SerializeField] GameObject antaLeft;

    /* Angolo oltre il quale l'anta sar� considerata APERTA */
    [SerializeField] float colliderActivactionRange;


    /* ---- DATI PER LE ANTE ---- */
    float dragObjectRight_MinZ = 0.07f;
    [HideInInspector] public float dragObjectRight_MaxZ = 0.23f;
    float antaRightClosed = 0f;
    [HideInInspector] public float antaRightOpen = 180f;
    float rightMinLimit = -2f;

    float antaRightOpenFixed = 180f;

    float dragObjectLeft_MinZ = -0.076f;
    [HideInInspector] public float dragObjectLeft_MaxZ = -0.276f;
    float antaLeftClosed = 0f;
    [HideInInspector] public float antaLeftOpen = -180f;
    float leftMinLimit = -2f;

    float antaLeftOpenFixed = -180f;
    /* ---- DATI PER LE ANTE ---- */


    /* Distanza per il quale oltre verr� tolto il grab. */
    [Header("Distance:")]  public float DropDistance = 5f;

    /* Variabili interni. */
    float currentDistanceRight;
    float currentDistanceLeft;

    /* Per capire se la scatola � aperta o chiusa. */
    public bool isClosed = true;
    bool rightClosed = true;
    bool leftClosed = true;


    void Start()
    {
        // RIGHT 
        DragObjectRight.position = antaRight.transform.GetChild(0).position;

        // LEFT
        DragObjectLeft.position = antaLeft.transform.GetChild(0).position;

        IgnoreColliders(antaRight, "ColliderEsterni");
        IgnoreColliders(antaLeft, "ColliderEsterni");

        StartCoroutine(LateStart(0.01f));
    }

    IEnumerator LateStart(float waitTime)
    {
        if (isClosed == false)
        {
            yield return new WaitForSeconds(waitTime);
            isClosed = true;
            yield return new WaitForSeconds(0.1f);
            isClosed = false;
        }
    }

    public void Trascina(string name) //left or right
    {

        float angolo_calcolato_dx;
        float angolo_calcolato_sx;
        float is_possibile;

        if (name == "right")
        {

            DragObjectRight.GetComponent<Collider>().enabled = false;

            currentDistanceRight = Vector3.Distance(DragObjectRight.transform.position, DragObjectRight.transform.parent.position);

            if (currentDistanceRight <= DropDistance)
            {
                /* Angolo calcolato � l'apertura in gradi alla quale si dovr� settare l'anta. */
                angolo_calcolato_dx = map_signal_right(DragObjectRight.localPosition.z);

                is_possibile = angolo_calcolato_dx;

                /* is_possibile � per far s� che l'anta rimanga tra i 0 e 180 gradi */
                if (!(is_possibile > 180 || is_possibile < 0))
                    antaRight.transform.localRotation = Quaternion.Euler(angolo_calcolato_dx, 0, 0);
                else if (is_possibile > 180)
                    antaRight.transform.localRotation = Quaternion.Euler(antaRightOpenFixed, 0, 0);
                else if (is_possibile < 0)
                    antaRight.transform.localRotation = Quaternion.Euler(antaRightClosed, 0, 0);
            }
            else
            {
                Rilascia("right");
            }
        }

        if (name == "left")
        {

            DragObjectLeft.GetComponent<Collider>().enabled = false;

            currentDistanceLeft = Vector3.Distance(DragObjectLeft.transform.position, DragObjectLeft.transform.parent.position);

            if (currentDistanceLeft <= DropDistance)
            {
                angolo_calcolato_sx = map_signal_left(DragObjectLeft.localPosition.z);
                is_possibile = angolo_calcolato_sx * -1;

                if (!(is_possibile > 180 || is_possibile < 0))
                    antaLeft.transform.localRotation = Quaternion.Euler(angolo_calcolato_sx, 0, 0);
                else if (is_possibile > 180)
                    antaLeft.transform.localRotation = Quaternion.Euler(antaLeftOpenFixed, 0, 0);
                else if (is_possibile < 0)
                    antaLeft.transform.localRotation = Quaternion.Euler(antaLeftClosed, 0, 0);
            }
            else
            {
                Rilascia("left");
            }
        }

    }

 
    public void Rilascia(string name)
    {

        /* Vado a risettare i valori della "mappatura" per far s� che quando riprendo il grab l'anta rimanga nella stessa posizione. */
        if (name == "right")
        {

            DragObjectRight.GetComponent<Collider>().enabled = true;

            float v_right = (Mathf.Asin(antaRight.transform.localRotation.x) * 2) * 180 / Mathf.PI;

            DragObjectRight.position = antaRight.transform.GetChild(0).position;
            DragObjectRight.localRotation = Quaternion.Euler(0, 180, 0);

            dragObjectRight_MaxZ = DragObjectRight.localPosition.z;

            if (v_right > rightMinLimit)
                antaRightOpen = rightMinLimit;
            else
                antaRightOpen = v_right;

            antaRightOpen = v_right;

            /* Controllo per chiudere bene */
            if (antaRightOpen < 10)
            {
                dragObjectRight_MinZ = 0.07f;
                dragObjectRight_MaxZ = 0.23f;
                antaRightClosed = 0f;
                antaRightOpen = 180f;
                rightMinLimit = -2f;
                antaRight.transform.localRotation = Quaternion.Euler(antaRightClosed, 0, 0);
            }

            /* Se l'inclinazione dell'anta supera quella impostata allora � APERTA (closed � false) */
            if ((Mathf.Asin(antaRight.transform.localRotation.x) * 2 * 180 / Mathf.PI) > colliderActivactionRange)
            {
                rightClosed = false;
                antaRight.GetComponent<BoxCollider>().enabled = false;
            }
            else
            {
                rightClosed = true;
                antaRight.GetComponent<BoxCollider>().enabled = true;
            }
                
        }

        if (name == "left")
        {

            DragObjectLeft.GetComponent<Collider>().enabled = true;

            float v_left = (Mathf.Asin(antaLeft.transform.localRotation.x) * 2) * 180 / Mathf.PI;

            DragObjectLeft.position = antaLeft.transform.GetChild(0).position;
            DragObjectLeft.localRotation = Quaternion.Euler(0, 180, 0);

            dragObjectLeft_MaxZ = DragObjectLeft.localPosition.z;

            if (v_left > leftMinLimit)
                antaLeftOpen = leftMinLimit;
            else
                antaLeftOpen = v_left;

            /* Controllo per chiudere bene */
            if (antaLeftOpen > -10)
            {
                dragObjectLeft_MinZ = -0.076f;
                dragObjectLeft_MaxZ = -0.276f;
                antaLeftClosed = 0f;
                antaLeftOpen = -180f;
                leftMinLimit = -2f;
                antaLeft.transform.localRotation = Quaternion.Euler(antaLeftClosed, 0, 0);
            }

            /* Se l'inclinazione dell'anta supera quella impostata allora � APERTA (closed � false) */
            if (Mathf.Abs(Mathf.Asin(antaLeft.transform.localRotation.x) * 2 * 180 / Mathf.PI) > colliderActivactionRange)
            {
                leftClosed = false;
                antaLeft.GetComponent<BoxCollider>().enabled = false;
            }
            else
            {
                leftClosed = true;
                antaLeft.GetComponent<BoxCollider>().enabled = true;
            }
                
        }

        /* Se entrambe le ante sono chiuse allora la scatola � chiusa completamente. */
        if (leftClosed && rightClosed)
            isClosed = true;
        else
            isClosed = false;
    }

    /* ---- FUNZIONI ---- */

    /* Calcola l'inclinazione in gradi da impostare all'anta destra (right). */
    float map_signal_right(float val)
    {
        return (val - dragObjectRight_MinZ) * (antaRightOpen - antaRightClosed) / (dragObjectRight_MaxZ - dragObjectRight_MinZ) + antaRightClosed;
    }

    /* Calcola l'inclinazione in gradi da impostare all'anta sinistra (left). */
    float map_signal_left(float val)
    {
        return (val - dragObjectLeft_MinZ) * (antaLeftOpen - antaLeftClosed) / (dragObjectLeft_MaxZ - dragObjectLeft_MinZ) + antaLeftClosed;
    }

    void IgnoreColliders(GameObject gameObject, string SearchColliderToIgnore)
    {
        GameObject colliderParent = GameObject.Find(SearchColliderToIgnore);
        Collider[] colliders = colliderParent.GetComponentsInChildren<Collider>();
        Collider selfCollider = gameObject.GetComponent<Collider>(); // cache fuori dal loop
        foreach (Collider childCollider in colliders)
        {
            Physics.IgnoreCollision(selfCollider, childCollider);
        }
    }

}
