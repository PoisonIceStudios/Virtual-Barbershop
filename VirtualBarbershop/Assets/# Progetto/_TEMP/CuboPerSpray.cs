using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuboPerSpray : MonoBehaviour
{
    // Start is called before the first frame update


    void OnTriggerEnter(Collider other)
    {
        Debug.Log("1");
    }

    void OnParticleTrigger()
    {
        Debug.Log("2");
    }

    void OnParticleCollision(GameObject other)
    {
        Physics.IgnoreCollision(other.GetComponent<Collider>(), this.GetComponent<Collider>(), true);
        Debug.Log("3");
    }

    void Start()
    {

    }

    void Update()
    {
        
    }
}
