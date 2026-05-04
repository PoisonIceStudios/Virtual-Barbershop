using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScatolaDynamic : MonoBehaviour
{
    
    /* Rotazione oltre la quale la scatola verr� considerata come rovesciata (perci� gli oggetti cadranno) */
    [SerializeField] float triggerRotation;
    public bool rovesciata;

    /* GameObject della scatola principale. */
    [SerializeField] GameObject mainScatola;
    Rigidbody rigidbodyScatola;

    private Rigidbody cachedRigidbodyScatola;
    private Scatola cachedScatolaScript;
    private Renderer cachedRenderer;

    /* Gli oggetti che entrano nella scatola verranno inseriti in questa lista. */
    public List<GameObject> listItems = new List<GameObject>();

    /* Quando la scatola � in movimento verr� messa a TRUE. */
    public bool inMovement;

    /* Numero di cicli totali di controllo per capire se la scatola si sta muovendo davvero oppure se sta solo tremando. */
    [SerializeField] int cicliContatoreMovimento;
    [SerializeField] int cicliContatoreFermo;

    int contatore_movimento;
    int contatore_fermo;

    /* La scatola si colorer� di ROSSO o VERDE se si sta muovendo o no. */
    [SerializeField] bool debugMode;
    [SerializeField] TextMesh textMeshDebug;
    [SerializeField] TextMesh textMesh2Debug;

    /* Calcolo per capire se la scatola si sta muovendo nello spazio (coordinate) */
    Vector3 lastPos;
    public float deltaPosition;

    /* Valore oltre per il quale la scatola verr� considerata in MOVIMENTO (basato su delta position) */
    [SerializeField] float deltaTriggerValue = 0.15f;
    [SerializeField] float rigidbodyTriggerValue = 0.1f;

    /* Controller VR */
    public BNG.HandController controllerSinistro;
    public BNG.HandController controllerDestro;

    /* Debug */
    public bool inGrab;

    public List<BNG.InputAxis> inputAxis = new List<BNG.InputAxis>() { BNG.InputAxis.LeftThumbStickAxis };

    private void Awake()
    {
        controllerSinistro = GameObject.FindGameObjectWithTag("LeftController").GetComponent<BNG.HandController>();
        controllerDestro = GameObject.FindGameObjectWithTag("RightController").GetComponent<BNG.HandController>();
    }

    void Start()
    {
        StartCoroutine(LateStart(0.02f));
    }

    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        /* Di base non � rovesciata e nemmeno in movimento. */
        rovesciata = false;
        inMovement = false;
        inGrab = false;

        /* Inizializzo contatore */
        contatore_movimento = 0;
        contatore_fermo = 0;

        /* Prendo il rigidbody della scatola principale. */
        rigidbodyScatola = mainScatola.GetComponent<Rigidbody>();
        cachedRigidbodyScatola = rigidbodyScatola;
        cachedScatolaScript = mainScatola.GetComponent<Scatola>();
        cachedRenderer = mainScatola.GetComponent<Renderer>();

        /* Coordinate scatola. */
        lastPos = mainScatola.transform.localPosition;

        /* Delta Position � la differenza di posizione tra prima e dopo (frame) della scatola. */
        deltaPosition = 0f;
    }

    public bool CheckPlayerMovement(float AxisDeadZone)
    {
        if (GetAxisInput().y > AxisDeadZone || GetAxisInput().y < -AxisDeadZone || GetAxisInput().x > AxisDeadZone || GetAxisInput().x < -AxisDeadZone)
        {
            return true;
        } else
        {
            return false;
        }
    }

    private void FixedUpdate()
    {
        deltaPosition = Mathf.Abs((lastPos.magnitude - mainScatola.transform.localPosition.magnitude) * 1000);
    }

    private void Update()
    {
        /* Calcolo la differenza di posizione in valore assoluto cos� � sempre positiva, inoltre moltiplico per 1000 per avere un valore alto. */
        deltaPosition = Mathf.Abs((lastPos.magnitude - mainScatola.transform.localPosition.magnitude) * 1000);

        /* Controllo la rotazione della scatola per impostare se � rovesciata. */
        checkRotation();

        /* Controllo per vedere se la scatola si sta muovendo e dunque togliendo il rigidbody agli oggetti nella scatola. */
        checkVelocity();

        /* Se sto grabbando la scatola da lontano tolgo il rigidbody a tutti. */

        /* Controllo se ho la scatola in mano. Se cos� fosse gli tolto la gravit� per evitare che cadda in continuazione. */
        /* Inoltre finch� ce l'ho in mano le collisioni saranno continue. */
        checkBox();

        lastPos = mainScatola.transform.localPosition;

        /* Debug rigidbody */
        if (debugMode)
        {
            if (cachedRigidbodyScatola.linearVelocity.magnitude * 1000 > rigidbodyTriggerValue)
            {
              Debug.Log("<color=red>" + cachedRigidbodyScatola.linearVelocity.magnitude * 1000 + "</color>");
            }
            else
            {
              Debug.Log("<color=green>" + cachedRigidbodyScatola.linearVelocity.magnitude * 1000 + "</color>");
            }
        }
    }

    void checkRotation()
    {

        /* Ottengo l'inclinazione della scatola in gradi (mi baso sull'asse Z) */
        float gradi = Mathf.Abs(this.transform.parent.localRotation.eulerAngles.z);

        /* Per correggere inclinazione. */
        if (gradi > 180)
            gradi = Mathf.Abs(gradi - 360);

        /* Se i gradi di inclinazione sono maggiori a quelli impostati e ROVESCIATA � FALSO allora imposto */
        /* rovesciata come VERO (perch� � ribaltata e gli oggetti dovranno cadere) */
        if (gradi > triggerRotation && !rovesciata)
        {
            // this.transform.localPosition = new Vector3(this.transform.localPosition.x, -90, this.transform.localPosition.z);
            rovesciata = true;
        }
        else if (gradi < triggerRotation && rovesciata)
        {
            // this.transform.localPosition = new Vector3(this.transform.localPosition.x, -0.34f, this.transform.localPosition.z);
            rovesciata = false;
        }

    }

    void checkVelocity()
    {
        /* Controllo tutti gli oggetti nella lista "listItems" (cio� quelli dentro la scatola) e se ne prendo uno col controller vr */
        /* (mano destra o sinistra) allora lo tolgo dalla lista e lo metto fuori (non pi� come figlio della scatola) */
        for (int i = listItems.Count - 1; i >= 0; i--)
        {
            GameObject item = listItems[i];
            Rigidbody itemRb = item.GetComponent<Rigidbody>();

            /* Se il rigidbody ESISTE */
            if (itemRb != null)
            {
                /* Se ce l'ho in una delle due mani allora lo tolgo dalla lista e anche come figlio. */
                if (item == controllerSinistro.PreviousHeldObject || item == controllerDestro.PreviousHeldObject)
                {
                    item.transform.parent = null;
                    //itemRb.interpolation = RigidbodyInterpolation.None; // MODIFICA DAMIANO
                    itemRb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    item.GetComponent<BNG.Grabbable>().RemoteGrabbable = true;
                    listItems.RemoveAt(i);
                }
            }
        }

        /* Se la scatola � CHIUSA allora tolgo il rigidbody a tutti gli oggetti dentro e anche se mi muovo io con il corpo*/
        if ((cachedScatolaScript.isClosed || cachedRigidbodyScatola.linearVelocity.magnitude * 1000 > rigidbodyTriggerValue) || CheckPlayerMovement(0.001f) == true)
        {
            foreach (GameObject item in listItems)
            {
                if (item.GetComponent<Rigidbody>() != null && cachedScatolaScript.isClosed)
                {
                    if (item.GetComponent<Rigidbody>().linearVelocity.magnitude < 0.1f)
                    {
                        Destroy(item.GetComponent<Rigidbody>());
                    }
                }
                else if (item.GetComponent<Rigidbody>() != null)
                {
                    Destroy(item.GetComponent<Rigidbody>());
                }
            }

            if (!inMovement)
                inMovement = true;
        }
        else
        {
            /* !!!! FORSE DA MODIFICARE CON VELOCITA' RIGIDBODY !!!! */ //  || (mainScatola.GetComponent<Rigidbody>().velocity.magnitude * 1000) > 1f
            /* Se la scatola si sta muovendo pi� di un certo valore */
            if ((deltaPosition > deltaTriggerValue) && !inMovement)
            {

                /* Quando supero il cicloContatore allora considero la scatola in MOVIMENTO e tolgo il rigidbody a tutti gli oggetti nella lista (dunque gli oggetti nella scatola).  */
                if ((contatore_movimento > cicliContatoreMovimento))
                {
                    foreach (GameObject item in listItems)
                    {
                        if (item.GetComponent<Rigidbody>() != null)
                        {
                            Destroy(item.GetComponent<Rigidbody>());
                        }
                    }

                    inMovement = true;
                }
                else
                    contatore_movimento++;

            }
            else
            {
                contatore_movimento = 0;
            }

            /* !!!! FORSE DA MODIFICARE CON VELOCITA' RIGIDBODY !!!! */
            /* Quando la velocit� spaziale sar� INFERIORE al valore impostato significa che saremo fermi. */
            if ((deltaPosition <= deltaTriggerValue) && inMovement)
            {

                if (contatore_fermo > cicliContatoreFermo)
                {
                    foreach (GameObject item in listItems)
                    {
                        /* Se l'oggetto non ha rigidbody allora glielo rimetto e in pi� imposto il rigidbody nel grabbable. */
                        if (item.GetComponent<Rigidbody>() == null)
                        {
                            item.AddComponent<Rigidbody>();
                            item.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Extrapolate;
                            item.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                            item.GetComponent<BNG.Grabbable>().rigid = item.GetComponent<Rigidbody>();
                            item.GetComponent<Rigidbody>().mass = 0f;
                            item.GetComponent<BNG.Grabbable>().RemoteGrabbable = false; // DAMIANO
                        }
                    }

                    inMovement = false;
                }
                else
                    contatore_fermo++;
            }
            else
            {
                contatore_fermo = 0;
            }

        }


        /* Solo per DEBUG. Se � in movimento la scatola si colora di ROSSO (cio� oggetti FERMI) */
        /* Se � VERDE allora gli oggetti sono liberi. */



        textMeshDebug.text = (cachedRigidbodyScatola.linearVelocity.magnitude * 1000).ToString();

        if (debugMode && inMovement)
        {
            cachedRenderer.material.color = Color.red;
            textMeshDebug.color = Color.black;
            textMesh2Debug.text = (cachedRigidbodyScatola.linearVelocity.magnitude * 1000).ToString();
            textMesh2Debug.color = Color.black;
        }
        else if (debugMode && !inMovement)
        {
            cachedRenderer.material.color = Color.green;
            textMeshDebug.color = Color.blue;
        }
    }

    void checkGrabbable()
    {
        if (mainScatola !=controllerSinistro.PreviousHeldObject && mainScatola != controllerDestro.PreviousHeldObject)
        {
            if (mainScatola.GetComponent<Rigidbody>().linearVelocity.magnitude > 1f && !inMovement)
            {
                foreach (GameObject item in listItems)
                {
                    if (item.GetComponent<Rigidbody>() != null)
                    {
                        Destroy(item.GetComponent<Rigidbody>());
                    }

                }

                if (!inMovement)
                    inMovement = true;

                inGrab = true;
            }
        }
        else if (inGrab)
            inGrab = false;
    }

    void checkBox()
    {
        /* Se ho la scatola in mano allora gli tolto la gravit� per evitare che cadda */
        /* In pi� gli imposto le collisioni come continue. */
        if ((controllerSinistro.PreviousHeldObject == mainScatola || controllerDestro.PreviousHeldObject == mainScatola))
        {
            if (cachedRigidbodyScatola.useGravity)
                cachedRigidbodyScatola.useGravity = false;

            if (cachedRigidbodyScatola.collisionDetectionMode != CollisionDetectionMode.ContinuousDynamic)
                cachedRigidbodyScatola.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        else
        {
            if (!cachedRigidbodyScatola.useGravity)
                cachedRigidbodyScatola.useGravity = true;

            if (cachedRigidbodyScatola.collisionDetectionMode == CollisionDetectionMode.ContinuousDynamic)
                cachedRigidbodyScatola.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        /* L'oggetto che entra in collisione con il box collider trigger. */
        GameObject gameObjectItem = other.gameObject;

        /* Se l'oggetto ha un rigidbody e come TAG � uno di quelli indicati allora l'oggetto andr� come figlio */
        /* della scatola (cos� si sposter� assieme alla scatola) e in pi� lo aggiungo alla lista per far capire */
        /* al codice che l'oggetto � DENTRO la scatola. */
        if (other.GetComponent<Rigidbody>() != null && (other.tag == "AttrezziObject" || other.tag == "AllBayObject" || other.tag == "DynamicObject" || other.tag == "AllBayObjectTutorial"))
        {
            /* Se NON CE L'HO in una delle due mani allora lo aggiungo alla lista e lo metto come figlio */
            if ((controllerSinistro.PreviousHeldObject != gameObjectItem || controllerDestro.PreviousHeldObject != gameObjectItem))   // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< ERRORE !!!!!!!!!!!
            {

                /* Questo controllo � da fare perch� quando diventer� FIGLIO il OnTriggerEnter verr� eseguito 2 volte. */
                if (gameObjectItem.transform.parent == mainScatola.transform)
                {

                    /* Inserisco l'oggetto nella lista. */
                    /* Inoltre imposto l'interpolazione come EXTRAPOLATE per evitare che cada fuori dalla scatola durante movimenti veloci.  */
                    if (!listItems.Contains(gameObjectItem))
                    {
                        listItems.Add(gameObjectItem);
                        gameObjectItem.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Extrapolate;
                        gameObjectItem.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                        gameObjectItem.GetComponent<Rigidbody>().mass = 0f;
                        gameObjectItem.GetComponent<BNG.Grabbable>().RemoteGrabbable = false;
                    }
                        
                }

                /* Lo metto come figlio, questo va messo DOPO l'istruzione sopra perch� spostandosi come figlio */
                /* OnTriggerEnter viene eseguito 2 volte. */
                gameObjectItem.transform.parent = mainScatola.transform;
            
            }


        }


    }

    private void OnTriggerExit(Collider other)
    {
        /* L'oggetto che entra in collisione con il box collider trigger. */
        GameObject item = other.gameObject;

        /* Sparento l'oggetto e lo tolgo dalla lista. */
        if (item.GetComponent<Rigidbody>() != null && (other.tag == "AttrezziObject" || other.tag == "AllBayObject" || other.tag == "DynamicObject" || other.tag == "AllBayObjectTutorial"))
        {
            item.transform.parent = null;

            /* Se l'oggetto esce dalla scatola per qualche motivo lo tolgo dalla lista (sempre se presente) */
            if (listItems.Contains(item))
            {
                listItems.Remove(item);
                //item.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None; DAMIANO
                item.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;
                item.GetComponent<BNG.Grabbable>().RemoteGrabbable = true;
            }
                
        }
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

}
