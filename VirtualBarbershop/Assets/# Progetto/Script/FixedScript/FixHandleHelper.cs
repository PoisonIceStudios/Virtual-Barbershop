using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {

    /// <summary>
    /// This component is used to pull grab items toward it, and then reset it's position when not being grabbed
    /// </summary>
    public class FixHandleHelper : MonoBehaviour {

        public Rigidbody ParentRigid;

        /// <summary>
        /// The Transform that is following us
        /// </summary>
        public Transform HandleTransform;
        public float ReturnVelocity = 10f;

        Grabbable thisGrab;

        bool didRelease = false;
        Collider col;

        public float ReturnMinDistance = 0.1f;

        public bool FixClose;
        public bool IsPosition;
        public float CloseValue;

        public float RotPos;
        bool oneTime;


        void FixedUpdate()
        {
                if (IsPosition)
                {
                    RotPos = Mathf.Round(ParentRigid.transform.position.x * 100f) / 100f;
                } else
                {
                    RotPos = Mathf.Round(ParentRigid.rotation.eulerAngles.y);
                }

            if ((FixClose) && (RotPos == CloseValue) && (!thisGrab.BeingHeld))
            {
                if (oneTime)
                {
                    oneTime = false;
                    // Debug.Log("Chiusa");
                    ParentRigid.constraints = RigidbodyConstraints.FreezeAll;
                }
            }
            else
            {
                oneTime = true;
                ParentRigid.constraints = RigidbodyConstraints.None;
                HandleTransform.GetComponent<Rigidbody>().MovePosition(transform.position);
            }
        }


        void Start() {
            thisGrab = GetComponent<Grabbable>();
            thisGrab.CanBeSnappedToSnapZone = false;
            col = GetComponent<Collider>();

            if(col != null && ParentRigid != null && ParentRigid.GetComponent<Collider>() != null) {
                Physics.IgnoreCollision(ParentRigid.GetComponent<Collider>(), col, true);
            }
        }


        void Update() {

            if(!thisGrab.BeingHeld) {
                if(!didRelease) {

                    transform.localRotation = Quaternion.identity;

                    if (transform.localPosition != Vector3.zero)
                    {
                        float dist = Vector3.Distance(transform.localPosition, Vector3.zero);

                        if (dist >= ReturnMinDistance)
                        {
                            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime * ReturnVelocity);
                        } else
                        {
                            transform.localPosition = Vector3.zero;
                        }

                    } else
                    {
                        didRelease = true;
                        col.enabled = true;
                    }
                }

            }
            else {

                col.enabled = false;
                didRelease = false;

                if (thisGrab.BreakDistance > 0 && Vector3.Distance(transform.position, HandleTransform.position) > thisGrab.BreakDistance) {
                    thisGrab.DropItem(false, false);
                }

            }
        }

        private void OnCollisionEnter(Collision collision) {
            Physics.IgnoreCollision(col, collision.collider, true);
        }


    }
}
