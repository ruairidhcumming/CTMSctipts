using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collect : MonoBehaviour {


    void Start()
    {

    }

    void RandomPos()
    {
        Vector3 pos;
        //Vector3 siz;
        // Vector3 rot;
        //pos = new Vector3(Random.Range(-18, 18), 0.5f, Random.Range(-18, 18));
        //siz = new Vector3(Random.Range(0, 2.5f), 10.9f, Random.Range(0, 2.5f));
        //rot = new Vector3(0, Random.Range(0, 180),0);
       /// transform.position = transform.parent.position + pos;
        //transform.localScale = siz;

        //transform.eulerAngles=rot;
    }


    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "veh")
       {   //Debug.Log("collected");
// RandomPos();
        }
    }
}
