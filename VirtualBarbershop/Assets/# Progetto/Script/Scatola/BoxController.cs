using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using BNG;

public enum UpdateMode
{
    FixedUpdate,
    IntervalUpdate,
    Update,
    Coroutine
}

public enum FixedModelMode
{
    DestroyRigidbody,
    FixedJoint
}

public class BoxController : MonoBehaviour
{

    private RimuoviScatola rimuoviScatolaScript; //Controllo extra, si pu� rimuovere(?)

    [Header("Rigidbody: ")]
    [SerializeField] private Rigidbody boxRigidbody;
    private Grabbable boxRemoteGrabbable;

    [Header("Velocity: ")]
    [SerializeField] private float thresholdVelocity = 5f;
    [SerializeField] private float SpeedCollisionLimit = 10;

    [Header("Update: ")]
    [SerializeField] private float TimerUpdateOnStart = 1;
    private float TimerUpdateOnStartDiv;
    [SerializeField] private UpdateMode updateMode = UpdateMode.FixedUpdate;

    [Header("IntervalUpdate & Coroutine")]
    [SerializeField] private float updateInterval = 0.5f;

    [Header("Fixed Model Mode: ")]
    [SerializeField] private FixedModelMode fixedModelMode = FixedModelMode.DestroyRigidbody;

    [Header("Objetcs Physycs: ")]
    [SerializeField] private bool addContinuosDynamic = false;

    [Header("Colliders: ")]
    [SerializeField] private Collider[] extraColliders;

    private float timeSinceLastUpdate = 0f;

    [Header("Rotation: ")]
    [SerializeField] private float triggerRotation = 120f;

    private List<Transform> objects = new List<Transform>();

    private Vector3 prevPosition;
    private float currentSpeedCollision;
    private const float minDeltaTime = 0.0001f;

    [Header("Movements Input: ")]
    public List<BNG.InputAxis> inputAxis = new List<BNG.InputAxis>() { BNG.InputAxis.LeftThumbStickAxis };
    private const float axisMovement = 0.001f;

    [Header("Debug: ")]
    private bool isMoving = false;
    [SerializeField] private bool isClosed = false;
    private bool checkClosedOneTime;

    private bool isUpsideDown;

    [SerializeField] private bool debugMode;

    private bool isRightDoorClosed = true;
    private bool isLeftDoorClosed = true;

    [Header("Zone Drag:")]
    public Transform DragObjectLeft;
    public Transform DragObjectRight;

    [Header("Box Doors:")]
    [SerializeField] private GameObject antaLeft;
    [SerializeField] private GameObject antaRight;

    [SerializeField] private float colliderActivationRange;

    [Header("Distance:")] public float DropDistance = 5f;

    CollidersScatoleManager collidersScatoleManager;


    private void Awake()
    {
        collidersScatoleManager = GameObject.Find("# Script").GetComponent<CollidersScatoleManager>();
    }

    private void Start()
    {
        rimuoviScatolaScript = GameObject.FindGameObjectWithTag("RimuoviScatolaScript").GetComponent<RimuoviScatola>();

        DragObjectRight.position = antaRight.transform.GetChild(0).position;
        DragObjectLeft.position = antaLeft.transform.GetChild(0).position;

        IgnoreColliders(antaRight, "ColliderEsterni");
        IgnoreColliders(antaLeft, "ColliderEsterni");

        TimerUpdateOnStartDiv = TimerUpdateOnStart/2;

        timeSinceLastUpdate = updateInterval;

        if (updateMode == UpdateMode.Coroutine)
        {
            StartCoroutine(CoroutineUpdate());
        }

        boxRemoteGrabbable = boxRigidbody.gameObject.GetComponent<Grabbable>();
    }

    private void Update()
    {
        if (TimerUpdateOnStart > 0)
        {
            TimerUpdateOnStart = TimerUpdateOnStart - Time.deltaTime;
        }

        if (updateMode == UpdateMode.Update)
        {
            Inizialize();
        }

        if (updateMode == UpdateMode.IntervalUpdate)
        {
            timeSinceLastUpdate += Time.deltaTime;
            if (timeSinceLastUpdate >= updateInterval)
            {
                Inizialize();
                timeSinceLastUpdate = 0f;
            }
        }

        if (boxRemoteGrabbable.RemoteGrabbable != isClosed)
        {
            boxRemoteGrabbable.RemoteGrabbable = isClosed;
        } 

    }

    private void OnTriggerStay(Collider other)
    {
        if (TimerUpdateOnStart <= TimerUpdateOnStartDiv)
        {
    
            if (IsValidObject(other))
            {
                bool shouldAdd = !objects.Contains(other.transform) && !IsChildOfObjectInList(other.transform);
                if (other.transform.childCount > 0)
                {
                    Transform firstChild = other.transform.GetChild(0);
                    if (firstChild.tag == "ModelRight" || firstChild.tag == "ModelLeft")
                    {
                        if (objects.Contains(other.transform))
                        {
                            RemoveObjectFromList(other);
                        }
                    }
                    else if (shouldAdd)
                    {
                        AddObjectOnList(other);
                    }
                }
                else if (shouldAdd)
                {
                    AddObjectOnList(other);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsValidObject(other))
        {
            if (other.transform.childCount > 0)
            {
                Transform firstChild = other.transform.GetChild(0);
                if (firstChild.tag != "ModelRight" && firstChild.tag != "ModelLeft")
                {
                    RemoveObjectFromList(other);
                }
            }
            else
            {
                RemoveObjectFromList(other);
            }
        }
    }

    private void RemoveObjectFromList(Collider other)
    {
        objects.Remove(other.transform);
        other.transform.parent = null;
        other.gameObject.GetComponent<BNG.Grabbable>().RemoteGrabbable = true;
        EnableDisableExtraCollisions(other, false);

        FixedJoint joint = other.gameObject.GetComponent<FixedJoint>();
        if (joint != null)
        {
            joint.connectedBody = null;
            Destroy(joint);
        }
    }

    private void AddObjectOnList(Collider other)
    {
        objects.Add(other.transform);
        other.transform.parent = gameObject.transform;
        other.gameObject.GetComponent<BNG.Grabbable>().RemoteGrabbable = false;
        EnableDisableExtraCollisions(other, true);

        RemoveFromRimuoviScatola(other);
    }

    public void RemoveFromRimuoviScatola(Collider obj)
    {
        if (rimuoviScatolaScript.Triggers.Contains(obj))
        {
            rimuoviScatolaScript.RemoveFromList(obj);
        }
    }

    private bool IsChildOfObjectInList(Transform child)
    {
        foreach (Transform obj in objects)
        {
            if (child.IsChildOf(obj))
            {
                return true;
            }
        }

        return false;
    }

    private void FixedUpdate()
    {

        if (updateMode == UpdateMode.FixedUpdate)
        {
            Inizialize();
        }
    }

    private IEnumerator CoroutineUpdate()
    {
        while (true)
        {
            Inizialize();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void Inizialize()
    {
        float currentSpeed = CalculateSpeed(boxRigidbody);
        currentSpeedCollision = CalculateCollisionSpeed(boxRigidbody);

        if (!checkClosedOneTime)
        {
            CheckIsClosed();
            checkClosedOneTime = true;
        }


        if (TimerUpdateOnStart <= 0)
        {

            if (isClosed)
            {
                if (debugMode)
                    Debug.Log("<color=gray>Box Closed...</color>");
                IsMoving = true;
                return;
            }

            if (CheckUpsideDown(boxRigidbody))
            {
                if (debugMode)
                    Debug.Log("<color=gray>Box Upside Down...</color>");
                IsMoving = false;
                return;
            }

            if (CheckPlayerMovement(axisMovement))
            {
                if (debugMode)
                    Debug.Log("<color=gray>Moving player...</color>");
                IsMoving = true;
                return;
            }

            if (currentSpeed >= (thresholdVelocity) && !isMoving)
            {
                IsMoving = true;

                if (debugMode)
                    Debug.Log("<color=green>Speed: " + currentSpeed + "</color>");
            }

            if (currentSpeed < (thresholdVelocity) && isMoving)
            {
                IsMoving = false;

                if (debugMode)
                    Debug.Log("<color=red>Box not moving, velocity: " + currentSpeed + "</color>");

                if (currentSpeedCollision > SpeedCollisionLimit)
                {
                    IsMoving = true;

                    if (debugMode)
                        Debug.Log("<color=magenta>Collision: " + Mathf.Round(currentSpeedCollision) + "</color>");
                }
                else
                {
                    IsMoving = false;
                    if (debugMode)
                        Debug.Log("<color=red>Speed: " + currentSpeed + "</color>");
                }
            }
        }
    }

    public bool IsMoving
    {
        get { return isMoving; }

        set
        {
            if (value == isMoving)
                return;

            isMoving = value;
            FixedJoint(isMoving);
        }
    }

    private void FixedJoint(bool addOrRemove)
    {
        if (fixedModelMode == FixedModelMode.DestroyRigidbody)
        {
            foreach (Transform obj in objects)
            {
                Rigidbody rigidbody = obj.gameObject.GetComponent<Rigidbody>();
                BNG.SnapZone snapzone = null;

                if (obj.transform.parent != null)
                {
                    snapzone = obj.transform.parent.GetComponent<BNG.SnapZone>();
                }

                if (addOrRemove)
                {
                    if (rigidbody != null && snapzone == null)
                    {
                        Destroy(rigidbody);

                        obj.gameObject.GetComponent<BNG.Grabbable>().enabled = false;
                    }
                }
                else
                {
                    if (rigidbody == null)
                    {

                        obj.gameObject.AddComponent<Rigidbody>();

                        if (addContinuosDynamic)
                        {
                            obj.gameObject.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
                        }

                        obj.gameObject.GetComponent<BNG.Grabbable>().enabled = true;
                    }
                }
            }
        }

        if (fixedModelMode == FixedModelMode.FixedJoint)
        {
            foreach (Transform obj in objects)
            {
                FixedJoint joint = obj.gameObject.GetComponent<FixedJoint>();
                BNG.SnapZone snapzone = null;

                if (obj.transform.parent != null)
                {
                    snapzone = obj.transform.parent.GetComponent<BNG.SnapZone>();
                }

                if (addOrRemove)
                {
                    if (joint == null && snapzone == null)
                    {
                        joint = obj.gameObject.AddComponent<FixedJoint>();
                        joint.connectedBody = boxRigidbody;

                        obj.gameObject.GetComponent<BNG.Grabbable>().enabled = false;
                        obj.gameObject.GetComponent<BNG.Grabbable>().RemoteGrabbable = false;
                    }
                }
                else
                {

                    if (joint != null)
                    {
                    obj.gameObject.GetComponent<BNG.Grabbable>().enabled = true;
                    obj.gameObject.GetComponent<BNG.Grabbable>().RemoteGrabbable = true;
                    }
                }
            }
        }
    }

    private void EnableDisableExtraCollisions(Collider coll, bool truefalse)
    {
        foreach (Collider extraCollider in extraColliders)
        {
            Physics.IgnoreCollision(coll, extraCollider, !truefalse);
        }
    }


    public float CalculateSpeed(Rigidbody rigidbody)
    {
        float deltaTime = Mathf.Max(Time.deltaTime, minDeltaTime);
        float speed = Vector3.Distance(rigidbody.transform.position, prevPosition) / deltaTime;
        prevPosition = rigidbody.transform.position;
        return Mathf.Round(speed * 10);
    }

    public float CalculateCollisionSpeed(Rigidbody rigidbody)
    {
        float velocitySpeed = rigidbody.linearVelocity.magnitude;
        return velocitySpeed * 1000;
    }

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

    public bool CheckPlayerMovement(float AxisDeadZone)
    {
        if (GetAxisInput().y > AxisDeadZone || GetAxisInput().y < -AxisDeadZone || GetAxisInput().x > AxisDeadZone || GetAxisInput().x < -AxisDeadZone)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CheckUpsideDown(Rigidbody rigidbody)
    {
        bool upsideDown = false;
        float gradi = Mathf.Abs(rigidbody.gameObject.transform.localRotation.eulerAngles.z);
        if (gradi > 180)
            gradi = Mathf.Abs(gradi - 360);

        if (gradi > triggerRotation)
        {
            upsideDown = true;
        }
        else if (gradi < triggerRotation)
        {
            upsideDown = false;
        }

        return upsideDown;
    }

    private bool IsValidObject(Collider other)
    {
        return other.attachedRigidbody != null &&
               (other.tag == "AttrezziObject" || other.tag == "AllBayObject" || other.tag == "DynamicObject" || other.tag == "AllBayObjectTutorial");
    }


    // ----------------------------- Funzioni Ante -------------------------------------------

    public void Drag(string name)
    {
        Transform dragObject = name == "right" ? DragObjectRight : DragObjectLeft;
        GameObject anta = name == "right" ? antaRight : antaLeft;

        dragObject.GetComponent<Collider>().enabled = false;

        float currentDistance = Vector3.Distance(dragObject.position, dragObject.parent.position);

        if (currentDistance <= DropDistance)
        {
            float angle = MapSignal(dragObject.localPosition.z, name);
            if (name == "right" && angle >= 0 && angle <= 180)
            {
                anta.transform.localRotation = Quaternion.Euler(angle, 0, 0);
            }
            else if (name == "left" && angle <= 0 && angle >= -180)
            {
                anta.transform.localRotation = Quaternion.Euler(angle, 0, 0);
            }
        }
        else
        {
            Release(name);
        }
    }

    public void Release(string name)
    {
        Transform dragObject = name == "right" ? DragObjectRight : DragObjectLeft;
        GameObject anta = name == "right" ? antaRight : antaLeft;

        dragObject.GetComponent<Collider>().enabled = true;

        dragObject.position = anta.transform.GetChild(0).position;
        dragObject.localRotation = Quaternion.Euler(0, 180, 0);

        float currentAngle = Mathf.Asin(anta.transform.localRotation.x) * 2 * 180 / Mathf.PI;

        if (name == "right")
        {
            isRightDoorClosed = Mathf.Abs(currentAngle) <= colliderActivationRange;
        }
        else
        {
            isLeftDoorClosed = Mathf.Abs(currentAngle) <= colliderActivationRange;
        }

        isClosed = isRightDoorClosed && isLeftDoorClosed;
        anta.GetComponent<BoxCollider>().enabled = !isClosed;
    }


    void CheckIsClosed()
    {
        float currentAngleRight = Mathf.Asin(antaRight.transform.localRotation.x) * 2 * 180 / Mathf.PI;
        float currentAngleLeft = Mathf.Asin(antaLeft.transform.localRotation.x) * 2 * 180 / Mathf.PI;

        isRightDoorClosed = Mathf.Abs(currentAngleRight) <= colliderActivationRange;
        isLeftDoorClosed = Mathf.Abs(currentAngleLeft) <= colliderActivationRange;

        isClosed = isRightDoorClosed && isLeftDoorClosed;
    }

    private float MapSignal(float value, string name)
    {
        float minZ, maxZ, open, closed;
        if (name == "right")
        {
            minZ = 0.07f;
            maxZ = 0.23f;
            open = 180f;
            closed = 0f;
        }
        else
        {
            minZ = -0.076f;
            maxZ = -0.276f;
            open = -180f;
            closed = 0f;
        }
        return (value - minZ) * (open - closed) / (maxZ - minZ) + closed;
    }

    private void IgnoreColliders(GameObject gameObject, string searchColliderToIgnore)
    {
        GameObject colliderParent = GameObject.Find(searchColliderToIgnore);
        Collider[] colliders = colliderParent.GetComponentsInChildren<Collider>();
        foreach (Collider childCollider in colliders)
        {
            Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), childCollider);
        }
    }


}