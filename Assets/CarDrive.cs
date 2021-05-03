using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDrive : MonoBehaviour
{
    
    bool carying = false;
    GameObject CarriedGameObject;
    navigate nav;
    public Vector3 offset = new Vector3(0f, 1f, 0f);
    string collectTag = "Collect";
    // Start is called before the first frame update
    void Start()
    {
        nav = this.GetComponent<navigate>();


    }

    // Update is called once per frame
    void Update()
    {   
        if (carying == true)
        {
            CarriedGameObject.transform.position = nav.grabber.transform.position + offset;// + col.transform.localScale.magnitude);
            CarriedGameObject.transform.rotation = nav.grabber.transform.rotation;

            if (nav.arrived(nav.Agent) == true & !nav.stuck(nav.grabber, nav.Agent) & (nav.Agent.transform.position - nav.grabber.transform.position).magnitude < 1.5)
            {
                drop(CarriedGameObject,nav,nav.Base.transform.position - nav.grabber.transform.position);
                nav.Agent.destination = nav.Base.position + new Vector3(-20, 0, 0);
            }
        }
        
        //check if object is carying something
        if (carying == false & nav.mode == "idle")
        {   

            GameObject nearest = nav.NearestInList(nav.Agent, nav.GetTaggedAndVisible(collectTag));
            if (nearest)
            {
             
             nav.Agent.SetDestination(nearest.transform.position);

            }
            else
            {   
                nearest = nav.NearestInList(nav.Agent, nav.GetTaggedFromList(collectTag, nav.Team.GetComponent<TeamHandler>().visible));
                if (nearest)
                {

                    nav.Agent.SetDestination(nearest.transform.position);

                }
            }

        }
    }

    void drop(GameObject dropobj,navigate dropper, Vector3 dropTo, string newtag = "Thrown")//drop/throw a carried object and assign it a new tag 
    {   
      
        
            dropobj.transform.parent = null;
            dropobj.tag = newtag;
            dropobj.GetComponent<Rigidbody>().isKinematic = false;
            dropobj.GetComponent<Rigidbody>().useGravity = true;
            float force = dropobj.GetComponent<Rigidbody>().mass * 0.5f;
            dropobj.GetComponent<Rigidbody>().AddForce(force * dropTo, ForceMode.Impulse);
            dropper.i = 0;
            dropper.iold = 0;
            //Debug.Log("drop");
            carying = false;
            dropper.Agent.SetDestination(dropper.grabber.transform.position);
        
    }
    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("COLLISION");
    }
    void OnTriggerEnter(Collider col)
    {
        HandleTrigger(col, this.GetComponent<navigate>());
    }
    public void HandleTrigger(Collider col,navigate caller)
    {   
        // Behaviour script;
    
        GameObject hitobj = col.transform.root.gameObject;

        //handle collisions with vehicles. 
        // drop any carried objects
        if (hitobj.tag == "Vehicle")
        {
            //Debug.Log("Hit Vehicle");
            if (CarriedGameObject != null&& hitobj.GetComponent<navigate>().Team == caller.Team) {

                drop(CarriedGameObject,caller, Vector3.Normalize(caller.grabber.transform.forward + caller.grabber.transform.up), "Collect");
            }
            if (hitobj.GetComponent<navigate>().Team == null)
            {

                //hitobj.GetComponent<navigate>().Team = caller.Team;
                //hitobj.GetComponent<navigate>().Base= caller.Base;
                //hitobj.GetComponent<navigate>().Home = caller.Home;
                Debug.Log(caller.Base);
                Debug.Log(caller.Home);
                Debug.Log(hitobj);
                Debug.Log(caller.Team.GetComponent<TeamHandler>());
                caller.Team.GetComponent<TeamHandler>().InitialiseVehicle(hitobj, caller.Base, caller.Home);
                hitobj.GetComponent<navigate>().Agent.Warp(hitobj.transform.position);
                hitobj.GetComponent<navigate>().Agent.enabled = false;
                hitobj.GetComponent<navigate>().Agent.enabled = true;

                hitobj.GetComponent<navigate>().mode = "following";

                caller.Follower = hitobj.GetComponent<navigate>().grabber;
                hitobj.GetComponent<navigate>().Target =caller.grabber.transform;

                caller.Agent.Warp(caller.grabber.transform.position - 10 * caller.grabber.transform.forward);
                caller.Agent.SetDestination(caller.Home.position);
                caller.mode = "leading";
                caller.AgentSpeed = Mathf.Min(hitobj.GetComponent<navigate>().AgentSpeed, this.GetComponent<VehCFG>().AgentSpeed);
            }


        }
        if (col.gameObject.tag == "Collect" & !carying)// if the object hit is collectable and you are not carying anything
        {
            //Debug.Log("collected");
            carying = true;
            col.transform.position = caller.grabber.transform.position + offset;// + col.transform.localScale.magnitude);
            col.transform.rotation = caller.grabber.transform.rotation;
            col.transform.SetParent(caller.grabber.transform);
            // script = GetComponent<rotator>();
            //            script.enabled = false;
            CarriedGameObject = col.gameObject;
            CarriedGameObject.GetComponent<Rigidbody>().isKinematic = true;
            CarriedGameObject.GetComponent<Rigidbody>().useGravity = false;
            CarriedGameObject.tag = "Collected";
            nav.mode = "carying";
            caller.Agent.SetDestination(caller.Home.position);
        }
    }
}