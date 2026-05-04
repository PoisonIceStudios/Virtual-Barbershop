using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spazzatura : MonoBehaviour
{

    Sottotitoli TutorialScript;

    public BNG.HandController ControllerSinistro;
    public BNG.HandController ControllerDestro;

    GameObject OggettoLeft;
    GameObject OggettoRight;

    AudioSource AudioSpazzatura;
    bool oneTimeUpdate = false;


    void Start()
    {
        AudioSpazzatura = this.GetComponent<AudioSource>();

        if (GameObject.Find("# ScriptTutorial") != null)
        {
            TutorialScript = GameObject.Find("# ScriptTutorial").GetComponent<Sottotitoli>();
        }

        if ((ControllerSinistro == null) || (ControllerDestro == null))
        {
            Debug.LogError("Inserire le mani su CESINO UI");
        }
    }

    void Update()
    {
        if ((OggettoLeft == null) && (OggettoRight == null) && (oneTimeUpdate == true))
        {
            this.GetComponent<MeshRenderer>().enabled = false;
            oneTimeUpdate = false;
        }
    }


    void OnTriggerEnter(Collider other)
    {

        if ((other.gameObject.tag == "AttrezziObject") || (other.tag == "AllBayObject") || (other.tag == "Scatola") || (other.tag == "AllBayObjectTutorial"))
        {
            if (ControllerSinistro.PreviousHeldObject == other.gameObject)
            {
                OggettoLeft = other.gameObject;
            }

            if (ControllerDestro.PreviousHeldObject == other.gameObject)
            {
                OggettoRight = other.gameObject;
            }

            this.GetComponent<MeshRenderer>().enabled = true;
        }
    }


    void OnTriggerExit(Collider other)
    {
        if ((other.gameObject.tag == "AttrezziObject") || (other.tag == "AllBayObject") || (other.tag == "Scatola") || (other.tag == "AllBayObjectTutorial"))
        {
            if (other.gameObject == ControllerSinistro.PreviousHeldObject)
            {
                OggettoLeft = null;
            }

            if (other.gameObject == ControllerDestro.PreviousHeldObject)
            {
                OggettoRight = null;
            }

            if ((OggettoLeft == null) && (OggettoRight == null))
            {
                this.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if ((other.gameObject.tag == "AttrezziObject") || (other.tag == "AllBayObject") || (other.tag == "Scatola") || (other.tag == "AllBayObjectTutorial")) //DynamicObject NO
        {
            if ((ControllerSinistro.PreviousHeldObject != OggettoLeft) && (OggettoLeft != null))
            {
                if ((TutorialScript != null) && (OggettoLeft.gameObject.tag == "AllBayObjectTutorial"))
                {
                    TutorialScript.ProssimaFrase(3);
                }

                Destroy(OggettoLeft);
                AudioSpazzatura.Play();
                oneTimeUpdate = true;
            }

            if ((ControllerDestro.PreviousHeldObject != OggettoRight) && (OggettoRight != null))
            {
                if ((TutorialScript != null) && (OggettoRight.gameObject.tag == "AllBayObjectTutorial"))
                {
                    TutorialScript.ProssimaFrase(3);
                }

                Destroy(OggettoRight);
                AudioSpazzatura.Play();
                oneTimeUpdate = true;
            }
        }
    }
}
