using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class baseControlScript : MonoBehaviour
{
    public GameObject Resource;
    public GameObject Base;
    public Dictionary<string, float> Resources = new Dictionary<string, float>() ;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider col)
    {
        Debug.Log("triggered");
        var resource = col.gameObject.GetComponent<Resource>();
        //Debug.Log(resource.val);
        //Debug.Log(resource.res);
        if (resource)
        {
            Debug.Log(resource.val);
            //Debug.Log(resource.res);
            if (Resources.ContainsKey(resource.res)) {
                Resources[resource.res] += resource.val;
               
            }
            else
            {
                Resources.Add(resource.res, resource.val);
               
            }
            //Debug.Log(BaseResources);
        }
        else
        {
            Debug.Log("triggered by not resource component");
        }
        Debug.Log(col.gameObject.tag);
        Debug.Log(col.gameObject.tag == "Thrown");
        if (col.gameObject.tag == "Thrown")
        {
            Debug.Log("new resource created, old destroyed");
            Instantiate(Resource, base.transform.position + new Vector3(Random.Range(0, 100), 2000, Random.Range(0, 200)), Quaternion.identity);
            Destroy(col.gameObject);
        }
                }
}
