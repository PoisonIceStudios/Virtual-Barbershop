using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtContinuos : MonoBehaviour {


        public Transform LookAt;
        public bool LookPlayer;

        public bool UseLerp = true;

        public float Speed = 20f;

        public bool UseUpdate = false;
        public bool UseLateUpdate = true;

        void Start()
        {
           if (LookPlayer)
            {
                LookAt = GameObject.Find("CenterEyeAnchor").transform;
            }
        }

        void Update() {
            if (UseUpdate) {
                lookAt();
            }
        }

        void LateUpdate() {
            if (UseLateUpdate) {
                lookAt();
            }
        }

        void lookAt() {

            if (LookAt != null) {

                if (UseLerp) {
                    Quaternion rot = Quaternion.LookRotation(LookAt.position - transform.position);

                    transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * Speed);
                }
                else {
                    transform.LookAt(LookAt, transform.forward);
                }
            }
        }
}

