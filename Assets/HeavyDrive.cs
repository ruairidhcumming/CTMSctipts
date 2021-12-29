using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyDrive : MonoBehaviour
{
    public bool carying = false;
    GameObject CarriedGameObject;
    navigate nav;
    public Vector3 offset = new Vector3(0f, 1f, 0f);
    string collectTag = "collect";
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
                    drop(CarriedGameObject, nav, nav.Base.transform.position - nav.grabber.transform.position);
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
                    nav.mode = "commanded";
                }
                else
                {
                    nearest = nav.NearestInList(nav.Agent, nav.GetTaggedFromList(collectTag, nav.Team.GetComponent<TeamHandler>().visible));
                    if (nearest)
                    {
                        nav.Agent.SetDestination(nearest.transform.position);
                        nav.mode = "commanded";
                    }
                    else
                    {//check for team vehicles which have been flipped TODO: or are stuck on my team
               
                        foreach (GameObject obj in nav.Team.GetComponent<TeamHandler>().visible)
                        {
                            if (obj.tag == "Vehicle" && obj.GetComponent<navigate>().Team == nav.Team && obj.GetComponent<navigate>().mode == "flipped")
                            { nav.Agent.SetDestination(obj.transform.position);
                                nav.mode = "commanded";

                            }

                        }
                        if (nav.mode =="idle"    )
                        //then check for unassigned vehicles which could be collected
                            foreach (GameObject obj in nav.Team.GetComponent<TeamHandler>().visible)
                        {   //if its a vehicle with no team
                         
                            if (obj.tag == "Vehicle" && obj.GetComponent<navigate>().Team == nav.Team.GetComponent<TeamHandler>().NoTeam)
                            {
                                nav.Agent.SetDestination(obj.transform.position);
                                nav.mode = "commanded";
                                }


                            }
                        if (nav.mode == "idle")//now look for flipped ENEMY vehicles to steal
                        {
                
                            foreach (GameObject obj in nav.Team.GetComponent<TeamHandler>().visible)
                           {
                                if (obj.tag == "Vehicle" && obj.GetComponent<navigate>().Team != nav.Team && obj.GetComponent<navigate>().mode == "flipped")
                                {
                                    nav.Agent.SetDestination(obj.transform.position);
                                    nav.mode = "commanded";

                                }

                            }

                        }
                    }

                }



            }
           
        
    }
    void OnTriggerEnter(Collider col)
    {
        HandleTrigger(col, this.GetComponent<navigate>());
    }
    public void HandleTrigger(Collider col, navigate caller)
    {
        GameObject hitobj = col.transform.root.gameObject;// Behaviour script;
        Debug.Log("Handling trigger from heavy vehicle");
        Debug.Log(hitobj.tag);
        

        //handle collisions with vehicles. 
        // drop any carried objects
        if (hitobj.tag == "Vehicle")
        {
            //Debug.Log("Hit Vehicle");
            //if im carrying something and the thing ive hit is on my team
            if (CarriedGameObject != null && hitobj.GetComponent<navigate>().Team == caller.Team)
            {
                //drop what im carrying 
                // heacy vehicles dont drop unless hit by similar heavy vehicle. need to check weight of colliding vehicles
                //drop(CarriedGameObject, caller, Vector3.Normalize(caller.grabber.transform.forward + caller.grabber.transform.up), "Collect");
            }
            if (hitobj.GetComponent<navigate>().Team == nav.Team.GetComponent<TeamHandler>().NoTeam)
            {

                //hitobj.GetComponent<navigate>().Team = caller.Team;
                //hitobj.GetComponent<navigate>().Base= caller.Base;
                //hitobj.GetComponent<navigate>().Home = caller.Home;
                //Debug.Log(caller.Base);
                //Debug.Log(caller.Home);
                //Debug.Log(hitobj);
                //Debug.Log(caller.Team.GetComponent<TeamHandler>());
                caller.Team.GetComponent<TeamHandler>().InitialiseVehicle(hitobj, caller.Base, caller.Home);
                hitobj.GetComponent<navigate>().Agent.Warp(hitobj.transform.position);
                hitobj.GetComponent<navigate>().Agent.enabled = false;
                hitobj.GetComponent<navigate>().Agent.enabled = true;
                hitobj.GetComponent<navigate>().Agent.transform.parent = nav.Team;
                hitobj.GetComponent<navigate>().mode = "following";

                caller.Follower = hitobj.GetComponent<navigate>().grabber;
                hitobj.GetComponent<navigate>().Target = caller.grabber.transform;

                caller.Agent.Warp(caller.grabber.transform.position - 10 * caller.grabber.transform.forward);
                caller.Agent.SetDestination(caller.Home.position);
                caller.mode = "leading";
                caller.AgentSpeed = Mathf.Min(hitobj.GetComponent<navigate>().AgentSpeed, this.GetComponent<VehCFG>().AgentSpeed);
            }


        }
        if (hitobj.tag == "Collect" & !carying)// if the object hit is collectable and you are not carying anything
        {
            //Debug.Log("collected");
            carying = true;
            //pick up object and hold it above the vehicle
            hitobj.transform.position = caller.grabber.transform.position + offset;// + col.transform.localScale.magnitude);
            hitobj.transform.rotation = caller.grabber.transform.rotation;
            hitobj.transform.SetParent(caller.grabber.transform);
            // script = GetComponent<rotator>();
            //            script.enabled = false;
            //stop the picked up object interacting with physics TODO: add weight of carried object to the vehicle to modle added weight of cargo somehow
            CarriedGameObject = hitobj.gameObject;
            CarriedGameObject.GetComponent<Rigidbody>().isKinematic = true;
            CarriedGameObject.GetComponent<Rigidbody>().useGravity = false;
            //set tags and modes
            CarriedGameObject.tag = "Collected";
            nav.mode = "carying";
            //take cargo home
            caller.Agent.SetDestination(caller.Home.position);
        }
        //if ive hit a fliped vehicle unflip it, check its team, set it to my team and send it to my base
        if (hitobj.gameObject.tag == "Vehicle")
        { navigate colNav = hitobj.GetComponent<navigate>();
            if (colNav.mode == "flipped")
            {
                //put vehicle back on its wheels 
                hitobj.transform.position = caller.grabber.transform.position 
                    + caller.grabber.transform.right * 2 
                    + caller.grabber.transform.forward * 4 + Vector3.up * 2;
                hitobj.transform.rotation = caller.grabber.transform.rotation;
                //take it from its old team and add it to this team
                if (colNav.Team != nav.Team)
                {
                    hitobj.GetComponent<navigate>().Team.GetComponent<TeamHandler>().loseVehicle(hitobj);

                    nav.setTeam(hitobj.transform);

                }
                else //if its already on my team just send it back to base 
                {
                    colNav.Agent.SetDestination(nav.Home.position);
                }
            }
        }
    }
    void drop(GameObject dropobj, navigate dropper, Vector3 dropTo, string newtag = "Thrown")//drop/throw a carried object and assign it a new tag 
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
}
