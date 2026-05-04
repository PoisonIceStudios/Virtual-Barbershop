using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;

public class Trigger_Spray : MonoBehaviour
{
    public Spray SprayScript;
    public ParticleSystem ps;
    public List<ParticleSystem.Particle> enter; 
    public int numEnter;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
        SprayScript = this.transform.parent.gameObject.GetComponent<Spray>();
        enter = new List<ParticleSystem.Particle>();
    }

    /*
     
    void OnParticleCollision(GameObject other)
    {

        if ((other.tag == "LongHair") || (other.tag == "ShortHair") || (other.tag == "Sopracciglia"))
        {
            other.gameObject.GetComponent<Collider>().isTrigger = true;
            Debug.Log("TRIGGERATO");
            other.GetComponent<MeshRenderer>().material.color = SprayScript.ColoreParticelle;
        }
    }

    public ParticleSystem ps;
    List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();

    // Use this for initialization
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    */

    void OnParticleTrigger()
    {
        numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);

        for (int i = 0; i < numEnter; i++)
        {
            ParticleSystem.Particle p = enter[i];
            Debug.Log(enter[i] + " - " + p);
            enter[i] = p;
        }

        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);

        Debug.Log("Working!");
    }



}
