using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class navigate : MonoBehaviour
{
    bool carying = false;
    GameObject CarriedGameObject;
    public GameObject grabber;
    public Transform Home;
    public Transform Target;
    public Transform Base;
    public Transform Team;
    public Transform Holding;

    bool selected = false;
    NavMeshAgent Agent;
    Vector3 offset = new Vector3(0f, 1f, 0f);
    // Start is called before the first frame update
    void Start()
    {
        Agent = this.GetComponent<NavMeshAgent>();
        carying = false;
        if (grabber == null) {
            grabber = this.GetComponent<GameObject>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        //check if object is selected
        //Debug.Log("I'm attached to " + gameObject.name);

        if (Team.GetComponent<TeamHandler>().selected.Contains(gameObject) & selected == false)
        {
            gameObject.Find("body").GetComponent<Renderer>().matreial = this.GetComponent<VehCFG>().HighlightMaterial;
            selected = true;

        }
        else if (!Team.GetComponent<TeamHandler>().selected.Contains(gameObject) & selected == true) {
            selected = false;
            gameObject.Find("body").GetComponent<Renderer>().matreial = this.GetComponent<VehCFG>().NormalMaterial;
        }
        //check if object is carying something
            if (carying == false)
        {
            GameObject nearest = NearestCollectable(Agent);
            if (nearest) { 
            float dist = Vector3.Distance(nearest.transform.position, this.transform.position);
                float sightRadius =  this.GetComponent<VehCFG>().sightRadius;
                //sctipt = this.GetComponent<VehCFG>;
              if (dist < sightRadius)
                {
                    Agent.SetDestination(nearest.transform.position);
                }
                }
            }
            
        
        if (carying == true)
        {
            CarriedGameObject.transform.position = grabber.transform.position + offset;// + col.transform.localScale.magnitude);
            CarriedGameObject.transform.rotation = grabber.transform.rotation;

            if ( arrived(Agent) ==true)
            {
                drop();
            }
        }

        
    }

    void drop()
    {
        if (CarriedGameObject != null)  //if (input.getaxis("drop")==1 & 
        {
            CarriedGameObject.transform.parent = null;
            CarriedGameObject.tag = "Thrown";
            CarriedGameObject.GetComponent<Rigidbody>().isKinematic = false;
            CarriedGameObject.GetComponent<Rigidbody>().useGravity = true;
            CarriedGameObject.GetComponent<Rigidbody>().AddForce(10f *( Base.position- grabber.transform.position), ForceMode.Impulse);
            //Debug.Log("drop");
            carying = false;
            Agent.SetDestination( grabber.transform.position);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        Behaviour script;
        //Debug.Log("hit");
        if (col.gameObject.tag == "Collect" & !carying)
        {
            //Debug.Log("collected");
            carying = true;
            col.transform.position = grabber.transform.position +offset;// + col.transform.localScale.magnitude);
            col.transform.rotation = grabber.transform.rotation;
            col.transform.SetParent(grabber.transform);
            // script = GetComponent<rotator>();
            //            script.enabled = false;
            CarriedGameObject = col.gameObject;
            CarriedGameObject.GetComponent<Rigidbody>().isKinematic = true;
            CarriedGameObject.GetComponent<Rigidbody>().useGravity = false;
            CarriedGameObject.tag = "Collected";
            Agent.SetDestination(Home.position);
        }
    }

    bool arrived(NavMeshAgent Agent) {
        if (!Agent.pathPending)

        { 
            if (Agent.remainingDistance <= Agent.stoppingDistance)
            {
                
                if (!Agent.hasPath || Agent.velocity.sqrMagnitude == 0f)
                {
                   
                    return true;
                }
            }
        }
        return false;
    }

    List<GameObject> GetCollectables(NavMeshAgent Agent)
    {
        List<GameObject> Collectables = new List<GameObject>();
        NavMeshPath path = new NavMeshPath();
        foreach (GameObject w in GameObject.FindGameObjectsWithTag("Collect"))
       
        {
            Agent.CalculatePath(w.transform.position,path );

            if (path.status == NavMeshPathStatus.PathComplete)
            {
                Collectables.Add(w);
            }
            //could mark unreachable targets here

        };

        return Collectables;
    }
    GameObject NearestCollectable(NavMeshAgent Agent )
    {
        GameObject nearest;
        List<GameObject>  Collectables = GetCollectables( Agent);
        nearest = null;
        foreach (GameObject w in Collectables)
        {
            if (nearest == null)
            {
                nearest = w;
            }

            else if (
                Vector3.Distance(grabber.transform.position, w.GetComponent<Collider>().ClosestPointOnBounds(grabber.transform.position)) <
                Vector3.Distance(grabber.transform.position, nearest.GetComponent<Collider>().ClosestPointOnBounds(grabber.transform.position)))
            {

                nearest = w;

            }
        }
        return (nearest);
    }
}
