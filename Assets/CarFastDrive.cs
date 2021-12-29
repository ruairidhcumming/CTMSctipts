using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarFastDrive : MonoBehaviour

{
    public float FlipForce = 1000;
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
                drop(CarriedGameObject, nav, nav.Base.transform.position - nav.grabber.transform.position);
                nav.Agent.destination = nav.Base.position + new Vector3(-20, 0, 0);
            }
        }

        //check if object is carying something
        if (carying == false & nav.mode == "idle"& nav.Team.GetComponent<TeamHandler>())
        {
            //for fast car check if there is a visible enemy to engage or idle vehicle to collect
         
            
            GameObject nearestVehicle = nav.NearestInList(nav.Agent
                , nav.GetTaggedFromList("Vehicle"
                    , nav.Team.GetComponent<TeamHandler>().visible
                    )
                );
            //if nearest is a friendly vehicle ignore it 
            if (nearestVehicle)
            {
                Debug.Log(nearestVehicle);
                Debug.DrawRay(transform.position, -transform.position+nearestVehicle.transform.position);
            }
            if (nearestVehicle & nearestVehicle.GetComponent<navigate>() & nearestVehicle.GetComponent<navigate>().Team == nav.Team.GetComponent<TeamHandler>().NoTeam)//if a vehicle is visible and doesnt have team assinged go and get it
            {
                Debug.Log("nearest exists but has no team");
                nav.Agent.SetDestination(nearestVehicle.transform.position);
            }
            else
            {//if no unclaimed vehicle is visible check for enemies to engage
                GameObject nearest = nav.NearestInList(nav.Agent, GetEnemyVehicles(nav.myCFG.sightRadius));
                if (nearest)
                {
                    Debug.Log("Found enemy vehicle");
                    float targetdist = Vector3.Distance(nearest.transform.position, this.transform.position);

                    ////sctipt = this.GetComponent<VehCFG>;
                    //if (targetdist < nav.sightRadius)
                    //{
                    nav.Agent.SetDestination(nearest.transform.position);
                    //}
                }
                else //if no nearest vehicle look for a thing to collect)
                {
                    Debug.Log("looking for collectables");
                    nearest = nav.NearestInList(nav.Agent, nav.GetTaggedFromList(collectTag, nav.Team.GetComponent<TeamHandler>().visible));
                    if (nearest)
                    {

                        nav.Agent.SetDestination(nearest.transform.position);

                    }
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
        float force = dropobj.GetComponent<Rigidbody>().mass * 10;
        dropobj.GetComponent<Rigidbody>().AddForce(force* (dropTo), ForceMode.Impulse);
        dropper.i = 0;
        dropper.iold = 0;
        //Debug.Log("drop");
        carying = false;
        dropper.Agent.SetDestination(dropper.grabber.transform.position);

    }
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("COLLISION");
        var contact  = collision.contacts[0];
        //if the collission happens with the front portion of the car and the other object is not in team or collectable 
        // we will call flip to apply a strong upward force to it
        if(Vector3.Dot(nav.grabber.transform.forward, contact.point- nav.grabber.transform.position) > 0)
        {
           // if (collision.transform.GetComponent<navigate>.Team!= nav.Team)
           // {
            Flip(collision.rigidbody, contact.point);
            Debug.Log("Flipped em!!");
           // }

        }
    }
    void OnTriggerEnter(Collider col)
    {   
        HandleTrigger(col, this.GetComponent<navigate>());
    }

    public void HandleTrigger(Collider col, navigate caller)
    {
        // Behaviour script;

        GameObject hitobj = col.transform.root.gameObject;

        //handle collisions with vehicles. 
        // drop any carried objects
        if (hitobj.tag == "Vehicle")
        {
            //Debug.Log("Hit Vehicle");
            if (CarriedGameObject != null && hitobj.GetComponent<navigate>().Team == caller.Team)
            {

                drop(CarriedGameObject, caller, caller.grabber.transform.forward + caller.grabber.transform.up, "Collect");
            }
            if (hitobj.GetComponent<navigate>().Team == null)
            {

                hitobj.GetComponent<navigate>().Team = caller.Team;
                hitobj.GetComponent<navigate>().Agent.Warp(hitobj.transform.position);
                hitobj.GetComponent<navigate>().Agent.enabled = false;
                hitobj.GetComponent<navigate>().Agent.enabled = true;

                hitobj.GetComponent<navigate>().mode = "following";

                caller.Follower = hitobj.GetComponent<navigate>().grabber;
                hitobj.GetComponent<navigate>().Target = caller.grabber.transform;

                caller.Agent.Warp(caller.grabber.transform.position - 10 * caller.grabber.transform.forward);
                caller.Agent.SetDestination(caller.Home.position);
                caller.mode = "leading";
                caller.AgentSpeed = Mathf.Min(hitobj.GetComponent<navigate>().AgentSpeed, this.GetComponent<VehCFG>().AgentSpeed);
            }


        }
        if (hitobj.GetComponent<Rigidbody>())
        { 
            if (col.gameObject.tag == "Collect" & !carying & hitobj.GetComponent<Rigidbody>().mass <= 5)// if the object hit is collectable and you are not carying anything
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
    public List<GameObject> GetEnemyVehicles(float visDist)
    {
        List<GameObject> objs = new List<GameObject>();
        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
        foreach (GameObject w in nav.GetTaggedFromList("Vehicle", nav.Team.GetComponent<TeamHandler>().visible))

        {


            if (Vector3.Distance(nav.grabber.transform.position, w.transform.position) < visDist & w.GetComponent<navigate>().Team != nav.Team)//if vible and not on our team
            {
                objs.Add(w);
            }


        };

        return objs;
    }
    void Flip(Rigidbody RB, Vector3 Point)
    {
         
        RB.AddExplosionForce(FlipForce, Point, 3, 3f); //.AddForceAtPosition
    }
}