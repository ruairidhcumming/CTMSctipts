using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public string res;
    public int val;
    float sinceThrown = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()

    {
        if (tag == "Thrown"){
            //check if resource has been thrown and start counting 
            sinceThrown = sinceThrown + Time.deltaTime;

        }
        if (sinceThrown > 5)
        {   //after 5 secconds reset it as collectable and reset sinceThrown time counter
            tag = "Collect";
            sinceThrown = 0;
        }
        Debug.DrawRay(transform.position, GetComponent<Rigidbody>().velocity, Color.green);
    }
}

