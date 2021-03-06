﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; // is this wheel attached to motor?
    public bool steering; // does this wheel apply steer angle?
}

public class navigate : MonoBehaviour
{   //pid values for drive function
    float p=0;
    float i=0;
    float d=0;
    float pold=0;
    float iold=0;
    float dold=0;
    float steeringAngle = 0;
    float SAold = 0;
    public float P;
    public float I;
    public float D;
    public float SteeringD;
    public float PEffect;
    public float IEffect;
    public float DEffect;
    public float AgentSpeed=5;
    public float AgentSlowSpeed = 0.5F;
    public List<AxleInfo> axleInfos; // the information about each individual axle
    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    public float maxSteeringAngle; // maximum steer angle the wheel can have
    public float steeringSwitch = 90;
    bool carying = false;
    GameObject CarriedGameObject;
    public GameObject grabber;
    public Transform Home;
    public Transform Target;
    public Transform Base;
    public Transform Team;
    public Transform Holding;
    public string mode = "idle";//options idle/commanded used to control behavior when commanded to move to point (ignore/divert to pickups etc)
    bool selected = false;
    public NavMeshAgent Agent;
    Vector3 offset = new Vector3(0f, 1f, 0f);
    // Start is called before the first frame update
    void Start()
    {
        Agent = this.gameObject.transform.Find("navmeshHolder").GetComponent<NavMeshAgent>();
        Agent.transform.parent = Team;
        carying = false;
        if (grabber == null) {
            grabber = this.GetComponent<GameObject>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        //check if vehicle is stuck
        Debug.Log(stuck(grabber, Agent));
       
        //check if object is selected and set selected status flag
        //Debug.Log("I'm attached to " + gameObject.name);
        
        if (Team.GetComponent<TeamHandler>().selected.Contains(gameObject) & selected == false)
        {
            GameObject body = GameObject.Find("body");
            body.GetComponent<Renderer>().material = this.GetComponent<VehCFG>().HighlightMaterial;
            selected = true;

        }
        else if (!Team.GetComponent<TeamHandler>().selected.Contains(gameObject) & selected == true) {
            selected = false;
            GameObject body = GameObject.Find("body");
            body.GetComponent<Renderer>().material = this.GetComponent<VehCFG>().NormalMaterial;
        }


        //check if object is carying something
        if (carying == false & mode  == "idle")
        {
            GameObject nearest = NearestCollectable(Agent);
            if (nearest) { 
            float targetdist = Vector3.Distance(nearest.transform.position, this.transform.position);
                float sightRadius =  this.GetComponent<VehCFG>().sightRadius;
                //sctipt = this.GetComponent<VehCFG>;
              if (targetdist < sightRadius)
                {
                    Agent.SetDestination(nearest.transform.position);
                }
                }
            }
            
        
        if (carying == true)
        {
            CarriedGameObject.transform.position = grabber.transform.position + offset;// + col.transform.localScale.magnitude);
            CarriedGameObject.transform.rotation = grabber.transform.rotation;

            if ( arrived(Agent) ==true & !stuck(grabber,Agent) & (Agent.transform.position-grabber.transform.position).magnitude < 1.5)
            {
                drop(Base);
               
            }
        }
        if (arrived(Agent) == true)
        {
            mode = "idle";
        }
        drive();

    }
    void drive()
    {
        Debug.DrawRay(grabber.transform.position, grabber.transform.forward * 10, Color.blue);
        //steering calculations
        steeringAngle = Vector3.SignedAngle(
            (-new Vector3(grabber.transform.position.x,0, grabber.transform.position.z)+
            new Vector3(Agent.transform.position.x,0,Agent.transform.position.z)), 
            new Vector3(grabber.transform.forward.x, 0, grabber.transform.forward.z),
            Vector3.up);

        Debug.DrawRay(grabber.transform.position, new Vector3(0, 0, steeringAngle), Color.green);
        //Debug.Log(steeringAngle);
        
        //pid calculations
        //p is forward backward distance 
        p = Vector3.Dot(
            (grabber.transform.position - new Vector3(0, grabber.transform.position.y, 0)) - (Agent.transform.position - new Vector3(0, Agent.transform.position.y, 0)),
            grabber.transform.forward
            );
        // old p simple linear distance on ground plane
        float dist  =((grabber.transform.position - new Vector3(0, grabber.transform.position.y, 0)) - (Agent.transform.position - new Vector3(0, Agent.transform.position.y, 0))) .magnitude;
        if (dist < 1& Agent.desiredVelocity.magnitude <0.1)
        {
            p = 0;
            dist = 0;
            i = 0;
            iold = 0;
        }
        //stop agent running away 
        if (dist > 5 )
        { Debug.Log("agent speed = 0");
            Agent.speed = AgentSlowSpeed;

        }
        else
        {
            Debug.Log("agent speed > 0");
            Agent.speed = AgentSpeed;

        }
        //    if (steeringAngle > steeringSwitch| steeringAngle < -steeringSwitch)
        //{
        //    //p = -p;
        //    steeringAngle = -steeringAngle;
        //}
        steeringAngle = -steeringAngle / 2;
        steeringAngle = steeringAngle - (steeringAngle + SAold) * SteeringD;
        SAold = steeringAngle;
        i = iold + p-dist*0.1f;
      
        d = p - pold;

        pold = p;
        iold = i;
        dold = d;
        PEffect = -p * P +dist *P/10;
        IEffect = -i * I;
        DEffect = d * D;

        float throttle = PEffect+ IEffect + DEffect;
        if (throttle < 0) 
        {
            steeringAngle = -steeringAngle;
        }
        //apply calculated values to wheels
        float motor = Mathf.Clamp( throttle, -maxMotorTorque, maxMotorTorque); //Input.GetAxis("Vertical");
        float steering = Mathf.Clamp(steeringAngle, -maxSteeringAngle, maxSteeringAngle); //Input.GetAxis("Horizontal");
        //Debug.Log(motor);
        
        
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
        }
    }
    void drop(Transform dropTo)
    {
        if (CarriedGameObject != null)  //if (input.getaxis("drop")==1 & 
        {
            CarriedGameObject.transform.parent = null;
            CarriedGameObject.tag = "Thrown";
            CarriedGameObject.GetComponent<Rigidbody>().isKinematic = false;
            CarriedGameObject.GetComponent<Rigidbody>().useGravity = true;
            CarriedGameObject.GetComponent<Rigidbody>().AddForce(10f *( dropTo.position- grabber.transform.position), ForceMode.Impulse);
            i = 0;
            iold = 0;
            //Debug.Log("drop");
            carying = false;
            Agent.SetDestination( grabber.transform.position);
        }
    }

    void OnTriggerEnter(Collider col)
    {
       // Behaviour script;
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
    bool stuck(GameObject vehicle, NavMeshAgent agent)
    {
        if (
            ((vehicle.transform.position - new Vector3(0, vehicle.transform.position.y, 0)) - (agent.transform.position - new Vector3(0, agent.transform.position.y, 0))).magnitude > 5
            &
            vehicle.transform.parent.GetComponent<Rigidbody>().velocity.magnitude < 0.1
            )

        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
