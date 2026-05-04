using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcchiCliente : MonoBehaviour
{

    public float Speed = 20f;

    public Vector3 RangeAFreeLook;
    public Vector3 RangeBFreeLook;

    public Vector3 RangeACameraLook;
    public Vector3 RangeBCameraLook;


    float timerchange;
    bool continueLook = false;

    Vector3 lookPos;

    public GameObject DebugObject;
    public bool DebugView;


    public Transform OcchioL;
    public Transform OcchioR;

    public Transform PalpebraL;
    public Transform PalpebraR;

    public float StartPalpebraRotation;
    public float FixRotazionePalpebre;

    Camera PlayerCamera;

    [HideInInspector]
    public bool AnimazioneInCorso;



    void Start()
    {
        timerchange = Random.Range(0.2f, 3f);
        lookPos = new Vector3(Random.Range(RangeAFreeLook.x, RangeBFreeLook.x), Random.Range(RangeAFreeLook.y, RangeBFreeLook.y), Random.Range(RangeAFreeLook.z, RangeBFreeLook.z));
        PlayerCamera = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
    }

    void LateUpdate()
    {

       if ( AnimazioneInCorso != true ) {

            Vector3 cam = PlayerCamera.transform.position;

            if (timerchange <= 0)
            {

                continueLook = false;

                if (cam.x > RangeACameraLook.x && cam.x < RangeBCameraLook.x && cam.y > RangeACameraLook.y && cam.y < RangeBCameraLook.y && cam.z > RangeACameraLook.z && cam.z < RangeBCameraLook.z) // Camera entro il Range;
                {

                    if (Random.Range(1, 3) == 1)
                    {
                        timerchange = Random.Range(3f, 10f);
                        continueLook = true;
                    }
                    else
                    {
                        lookPos = new Vector3(Random.Range(RangeAFreeLook.x, RangeBFreeLook.x), Random.Range(RangeAFreeLook.y, RangeBFreeLook.y), Random.Range(RangeAFreeLook.z, RangeBFreeLook.z));
                        timerchange = Random.Range(0.2f, 4f);
                    }

                }
                else
                {
                    lookPos = new Vector3(Random.Range(RangeAFreeLook.x, RangeBFreeLook.x), Random.Range(RangeAFreeLook.y, RangeBFreeLook.y), Random.Range(RangeAFreeLook.z, RangeBFreeLook.z));
                    timerchange = Random.Range(0.2f, 4f);
                    continueLook = false;
                }


            }
            else
            {
                timerchange = timerchange - Time.deltaTime;
            }

            if (DebugObject != null && DebugView == true) // Debug
            {
                Quaternion rotL = Quaternion.LookRotation(DebugObject.transform.position - OcchioL.transform.position);
                Quaternion rotR = Quaternion.LookRotation(DebugObject.transform.position - OcchioR.transform.position);
                OcchioL.transform.rotation = Quaternion.Slerp(OcchioL.transform.rotation, rotL, Time.deltaTime * Speed);
                OcchioR.transform.rotation = Quaternion.Slerp(OcchioR.transform.rotation, rotR, Time.deltaTime * Speed);
            }
            else
            {
                Quaternion rotL = Quaternion.LookRotation(lookPos - OcchioL.transform.position);
                Quaternion rotR = Quaternion.LookRotation(lookPos - OcchioR.transform.position);
                OcchioL.transform.rotation = Quaternion.Slerp(OcchioL.transform.rotation, rotL, Time.deltaTime * Speed);
                OcchioR.transform.rotation = Quaternion.Slerp(OcchioR.transform.rotation, rotR, Time.deltaTime * Speed);
            }

            if (continueLook == true)
            {
                if (cam.x > RangeACameraLook.x && cam.x < RangeBCameraLook.x && cam.y > RangeACameraLook.y && cam.y < RangeBCameraLook.y && cam.z > RangeACameraLook.z && cam.z < RangeBCameraLook.z) // Camera entro il Range;
                {
                    lookPos = cam;
                }
                else
                {
                    timerchange = 0;
                }
            }

            if (PalpebraL != null || PalpebraR != null)
            {
                Quaternion target1 = Quaternion.Euler(0, 180 - FixRotazionePalpebre, StartPalpebraRotation + OcchioL.transform.rotation.eulerAngles.x);
                Quaternion target2 = Quaternion.Euler(0, 180 + FixRotazionePalpebre, StartPalpebraRotation + OcchioR.transform.rotation.eulerAngles.x);
                PalpebraL.rotation = target1;
                PalpebraR.rotation = target2;
            }

        }

    }
}
