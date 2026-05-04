using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


    public class FunzioniMouse : MonoBehaviour
    {
        public List<BNG.InputAxis> inputAxis = new List<BNG.InputAxis>() { BNG.InputAxis.LeftThumbStickAxis };
        public InGameMenu InGameMenuScript;

        bool Grabbato;

        BNG.PlayerRotation PlayerRotation;

        float posizioneZ;
        float posizioneX;

        public float posizioneXFinale;
        public float posizioneZFinale;

        public Transform OggettoMouse;

        public GameObject Cursore;
        [HideInInspector]
        public Camera CameraCursore;

        RaycastHit HitInfo;

        public Canvas CanvasHairShop;
        public Canvas CanvasAllBay;

        public Scrollbar[] ScrollbarFinestre;

        public virtual Vector2 GetAxisInput()
        {
            Vector3 lastAxisValue = Vector3.zero;

            for (int i = 0; i < inputAxis.Count; i++)
            {
                Vector3 axisVal = BNG.InputBridge.Instance.GetInputAxisValue(inputAxis[i]);
                
                if (lastAxisValue == Vector3.zero)
                {
                    lastAxisValue = axisVal;
                }
            }
            return lastAxisValue;
        }

       void Start()
        {

            PlayerRotation = GameObject.FindFirstObjectByType<BNG.PlayerRotation>();
            CameraCursore = Cursore.GetComponentInChildren<Camera>();
            InGameMenuScript = this.gameObject.GetComponent<InGameMenu>();

            Grabbato = false;

            posizioneZ = 0.07312035f;
            posizioneX = -0.5452881f;

            EventSystem.current.SetSelectedGameObject(null);

            PosizioneMouse();

            Collider grabberCollider = GameObject.FindGameObjectWithTag("GrabberLeft").GetComponent<Collider>();
            // ignora la collisione tra il collider GravverLeft e l'oggetto mouse
             Physics.IgnoreCollision(grabberCollider, OggettoMouse.GetComponent<Collider>());

    }

    void PosizioneMouse()
        {

            Transform cameraTransform = CameraCursore.transform;
            Physics.Raycast(CameraCursore.transform.position, CameraCursore.transform.forward, out HitInfo, 1000.0f);

            posizioneXFinale = (posizioneX - OggettoMouse.localPosition.x) * 100;
            posizioneXFinale = 484.7f - (2 * posizioneXFinale * 484.7f / 8.907545f);  //9 o 8.907545

            posizioneZFinale = (posizioneZ - OggettoMouse.localPosition.z) * 100;  //30 o 29.89776 
            posizioneZFinale = (2 * posizioneZFinale * 923 / 29.89776f) - 923;

            Cursore.GetComponent<RectTransform>().anchoredPosition = new Vector2(posizioneZFinale, posizioneXFinale);

        }

        public void AttivaMouse()
        {
            Grabbato = true;
            InGameMenuScript.PausaNonCosentita = true;
            PlayerRotation.AllowInput = false;
        }

        public void DisattivaMouse()
        {
            Grabbato = false;
            InGameMenuScript.PausaNonCosentita = false;
            PlayerRotation.AllowInput = true;
        }

        
      

        void Update()
        {

            if (Grabbato == true)
            {

            Vector2 axisInput = GetAxisInput(); // calcola una sola volta per frame
            if (CanvasHairShop.enabled)
                ScrollbarFinestre[0].value += axisInput.y / 100f;

            if (CanvasAllBay.enabled)
                ScrollbarFinestre[1].value += axisInput.y / 100f;

            if (HitInfo.collider.gameObject.name != "Statici_2" ) 
                {
                    if (HitInfo.collider.GetComponent<Button>() != null)
                    {
                       HitInfo.collider.GetComponent<Button>().Select();
                    }
                    if (HitInfo.collider.GetComponent<Toggle>() != null)
                    {
                       HitInfo.collider.GetComponent<Toggle>().Select();
                    }
            }
                else
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }

                PosizioneMouse();
            }
        
        }
    }


