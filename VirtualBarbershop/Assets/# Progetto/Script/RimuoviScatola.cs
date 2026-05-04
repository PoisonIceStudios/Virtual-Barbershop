using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RimuoviScatola : MonoBehaviour
{

    public List<Collider> Triggers = new List<Collider>();


    public void RimuoviOggettiScatola()
    {
       for (int i = 0; i < Triggers.Count; i++)
       {
           var GameObj = Triggers[i].gameObject;
           Destroy(GameObj);
       }
       Triggers.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
       if (IsValidObject(other))
       {
           Triggers.Add(other);
       }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Triggers.Contains(other))
        {
          Triggers.Remove(other);
        }
    }

    public void RemoveFromList(Collider other)
    {
        if (Triggers.Contains(other))
        {
            Triggers.Remove(other);
        }
    }

    private bool IsValidObject(Collider other)
    {
        return !Triggers.Contains(other) &&
               (other.tag == "Scatola" || other.tag == "AttrezziObject" || other.tag == "AllBayObject" || other.tag == "DynamicObject" || other.tag == "AllBayObjectTutorial");
    }


}

