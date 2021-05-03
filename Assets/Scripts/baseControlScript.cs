using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class baseControlScript : MonoBehaviour
{
    public GameObject Resource;
    public GameObject Base;
    public Dictionary<string, float> Resources = new Dictionary<string, float>() ;
    public Material HighlightMaterial;
    public Material NormalMaterial;
    public Transform Team;
    bool selected = false;
    // Start is called before the first frame update
    void Start()
    {
        Team =  transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
    
        //check if object is selected and set selected status flag
        //Debug.Log("I'm attached to " + gameObject.name);
       
        if (Team.GetComponent<TeamHandler>().selected.Contains(gameObject) & selected == false)
        {
            Debug.Log(this.GetComponent<Renderer>().material);
            this.GetComponent<Renderer>().material = HighlightMaterial;
            selected = true;

        }
        else if (!Team.GetComponent<TeamHandler>().selected.Contains(gameObject) & selected == true)
        {
            selected = false;

            this.GetComponent<Renderer>().material = NormalMaterial;
        }
    }
    void OnTriggerEnter(Collider col)
    {
        //Debug.Log("triggered");
        var resource = col.gameObject.GetComponent<Resource>();
        
  
        if (resource)
        {
            //Debug.Log(resource.val);
            //Debug.Log(resource.res);
            if (Resources.ContainsKey(resource.res)) {
                Resources[resource.res] += resource.val;
               
            }
            else
            {
                Resources.Add(resource.res, resource.val);
               
            }
            //Debug.Log(BaseResources);
            //Debug.Log("new resource created, old destroyed");
            Instantiate(Resource, base.transform.position + new Vector3(Random.Range(0, 100), 2000, Random.Range(0, 200)), Quaternion.identity);
            Destroy(col.gameObject);
        }
        else
        {
            //Debug.Log("triggered by not resource component");
        }
        //Debug.Log(col.gameObject.tag);
        //Debug.Log(col.gameObject.tag == "Thrown");
        
      

        
    }
}
