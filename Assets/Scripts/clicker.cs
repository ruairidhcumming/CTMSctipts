using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clicker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Click");
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000))
            {   //Debug.Log("Click Hit");
                if (hit.transform)
                {

                    //Debug.Log("Click found object");
                    Select(hit.transform.gameObject);

                    
                }
            }
        }
    }
    void Select(GameObject go)
    {
        //Debug.Log("Click object name");
        //Debug.Log(go.tag);
        if (go.tag != "Terrain") // & go.GetComponent<Rigidbody>()) 
        {
            
           // Camera.main.GetComponent<camera_controler>().target = go;
           // Camera.main.GetComponent<camera_controler>().mode = "follow";

        }
        //set clicked object as main camera target

    }

}
