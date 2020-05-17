using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class baseControlScript : MonoBehaviour
{
    public GameObject Resource;
    public GameObject Base;
    public Dictionary<string, int> BaseResources = new Dictionary<string, int>() ;

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
            if (BaseResources.ContainsKey(resource.res)) {
                BaseResources[resource.res] += resource.val;
                //Debug.Log("added new resource");
            }
            else
            {
                BaseResources.Add(resource.res, resource.val);
                //Debug.Log("resource increased");
            }
            //Debug.Log(BaseResources);
        }
        //Debug.Log(col.gameObject.tag);
        if (col.gameObject.tag == "Thrown")
            //Debug.Log("new resource created, old destroyed");
            Instantiate(Resource, base.transform.position + new Vector3(Random.Range(0, 100), 2000, Random.Range(0, 200)), Quaternion.identity);
            Destroy(col.gameObject);

                }
}
