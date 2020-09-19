using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hold : MonoBehaviour {
    bool carying;
    GameObject CarriedGameObject;
    public GameObject grabber;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
      
	}

    void drop()
    {
         if (CarriedGameObject != null)  //if (Input.GetAxis("drop")==1 & 
       {
            CarriedGameObject.transform.parent = null;
            Debug.Log("drop");
            carying = false;
        }
    }
    void OnTriggerEnter(Collider col)
    { //Behaviour script;
        Debug.Log("hit");
        if (col.gameObject.tag == "Collect" & !carying)
        { Debug.Log("collected");
            carying = true;
            col.transform.position = grabber.transform.position + new Vector3(0f, 0f, 0f);// + col.transform.localScale.magnitude);
            col.transform.rotation = grabber.transform.rotation;
            col.transform.SetParent(grabber.transform);
// script = GetComponent<rotator>();
//            script.enabled = false;
            CarriedGameObject = col.gameObject;
        }
    }
}
