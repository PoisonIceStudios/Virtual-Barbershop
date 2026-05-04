using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class HelperMethods
{
    public static List<GameObject> GetChildren(this GameObject go)
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform tran in go.transform)
        {
            children.Add(tran.gameObject);
        }
        return children;
    }
}


public class TestStaticBatchingRealtime : MonoBehaviour
{
    public GameObject Cliente;



    private void Awake()
    {
        // ClienteCorrente = Instantiate(Clienti[0]); // DA TOGLIERE SOLO PER TESTARE STATIC BATCHING CON ISTANTIATE
        //ClienteCorrente.SetActive(false);
        //  ClienteCorrente.SetActive(true);
    }
    void Start()
    {

        if (Cliente != null)
        {
            Instantiate(Cliente); // DA TOGLIERE SOLO PER TESTARE STATIC BATCHING CON ISTANTIATE
            // Cliente.SetActive(false);
            // Cliente.SetActive(true);
        }
        else
        {
            GameObject[] ArrayStaticBatching = HelperMethods.GetChildren(this.gameObject).ToArray();
           

            for (int i = 0; i < ArrayStaticBatching.Length; i++)
            {
                Debug.Log(ArrayStaticBatching[i]);
            }

            StaticBatchingUtility.Combine(ArrayStaticBatching, ArrayStaticBatching[0]);
        }
    }

}
