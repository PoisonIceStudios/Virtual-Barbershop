using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;

public class Campanello : MonoBehaviour
{

    Transform Oggetto;
    public bool StartOneTime;
    AudioSource AudioPlay;


    public GameObject LeftHandModel;
    public GameObject RightHandModel;

    HandCollision LeftHandCollisionScript;
    HandCollision RightHandCollisionScript;


    public float test;

    void Start()
    {
        Oggetto = this.gameObject.transform;
        AudioPlay = this.gameObject.GetComponent<AudioSource>();
        LeftHandCollisionScript = LeftHandModel.GetComponent<HandCollision>();
        RightHandCollisionScript = RightHandModel.GetComponent<HandCollision>();
    }

    void Update()
    {

        test = Oggetto.localPosition.y;

        if (Oggetto.localPosition.y <= 0.22f)
        {
            if (StartOneTime == false)
            {
                StartOneTime = true;
                AudioPlay.Play();
            }
        } else
        {
            StartOneTime = false;
        }
    }

    public void AttivaSuono()
    {
        AudioPlay.Play();
    }


    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "GrabberLeft")
        {
            LeftHandCollisionScript.EnableCollisionOnAllPoses = true;
        }
        if (other.gameObject.tag == "GrabberRight")
        {
            RightHandCollisionScript.EnableCollisionOnAllPoses = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "GrabberLeft")
        {
            LeftHandCollisionScript.EnableCollisionOnAllPoses = false;
        }
        if (other.gameObject.tag == "GrabberRight")
        {
            RightHandCollisionScript.EnableCollisionOnAllPoses = false;
        }
    }

}

