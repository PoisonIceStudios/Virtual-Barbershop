using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corriere : MonoBehaviour
{
    ComputerOS ComputerOSScript;

    private void Start()
    {
        ComputerOSScript = GameObject.Find("# Script").GetComponent<ComputerOS>();
    }

    public void SpawnScatolaEvent()
    {
        ComputerOSScript.SpawnScatola();
    }


}
