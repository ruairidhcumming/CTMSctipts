using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using System.Security.Cryptography;
//using System.Net.Configuration;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; // is this wheel attached to motor?
    public bool steering; // does this wheel apply steer angle?
    public bool reverseSteering;// does this wheel steer in the opposite directon wrt direction of travel? e.g. rear wheel steering 
}

public class navigate : MonoBehaviour
{
    public string mode;//= "idle";//options idle/commanded/following used to control behavior when commanded to move to point (ignore/divert to pickups etc)
    public GameObject Follower;
    //pid values for drive function
    float p=0;
    public float i=0;
    float d=0;
    float pold=0;
    public float iold=0;
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
    public float throttle;
    public bool Brakes;
    //agent position controls 
    public float AgentSpeed=10;
    public float AgentSlowSpeed = 0.5F;
    public float stucktimer = 2f;
    public List<AxleInfo> axleInfos; // the information about each individual axle
    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    public float maxSteeringAngle; // maximum steer angle the wheel can have
    public float steeringSwitch = 90;
   // public bool carying = false;
   //public GameObject CarriedGameObject;
    public GameObject grabber;
    public Transform Home = null;
    public Transform Target;
    public Transform Base=null;
    public Transform Team = null;
    public Transform Holding;
    public VehCFG myCFG;
    public float sightRadius;
    public bool selected = false;
    public NavMeshAgent Agent;
    public NavMeshAgent AvoidanceAgent;
   
    public List<GameObject> teamVisible = new List<GameObject>();
    public List<GameObject> visible = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("nav start");
        Agent = this.gameObject.transform.Find("navmeshHolder").GetComponent<NavMeshAgent>();
        AvoidanceAgent = this.gameObject.transform.Find("body").GetComponent<NavMeshAgent>();
        Agent.transform.parent = Team;

        //turn agent off and on again
        Agent.enabled = false;
        Agent.enabled = true;
        //Agent.updatePosition = false;
        //Agent.updateRotation = false;
        //carying = false;
        if (grabber == null) {
            grabber = this.GetComponent<GameObject>();
        }
        myCFG = this.GetComponent<VehCFG>();
        sightRadius = myCFG.sightRadius;
        if (Home)
        {
            Agent.SetDestination(Home.position);
        }

    }

    // Update is called once per frame
    void Update()
    {   
        if (Team.GetComponent<TeamHandler>().Name =="NoTeam")
        {
            return;
        }
        //send visible objects to team
        visible = GetVisible(sightRadius);
        if (Team.GetComponent<TeamHandler>().selected.Contains(gameObject)&& selected == false)//
        {   Debug.Log(gameObject.name + "is selected");
            GameObject body = grabber;
            body.GetComponent<Renderer>().material = gameObject.GetComponent<VehCFG>().HighlightMaterial;
            selected = true;

        }
        else if (!Team.GetComponent<TeamHandler>().selected.Contains(gameObject)&& selected == true) // 
        {

            GameObject body = grabber;
            body.GetComponent<Renderer>().material = gameObject.GetComponent<VehCFG>().NormalMaterial;
            selected = false;
        }

        //set destination for follower 
        if (mode == "following" & Target != null)
        {   
            Vector3 targetDir = (grabber.transform.position-Target.position).normalized  ;

            Agent.SetDestination(Target.position+targetDir*5);

            //check if we are near the destination and break away

        }
        if (mode == "following" & Target == null)
        {
            mode = "idle";
        }
        //control speed for leader 
        if(mode == "leading" & Follower != null )
        {   
            AgentSpeed = Mathf.Min(this.GetComponent<VehCFG>().AgentSpeed,Follower.transform.parent.GetComponent<navigate>().Agent.speed);
            //Debug.Log(Follower.transform.parent.GetComponent<navigate>());
            //Debug.Log(AgentSpeed);
            if (arrived(Agent) == true)
            {
                //Debug.Log("arrived");
                Agent.SetDestination(Home.position + (Home.position - Base.position));
                mode = "commanded";
                //release follower
                Follower.transform.parent.GetComponent<navigate>().Agent.SetDestination(Base.position);
                addFollowerToTeam(Follower);
                //releaseFollower(Follower);
            }
        }
        if (mode == "leading" & Follower == null) 
        {
            // Agent.speed = this.GetComponent<VehCFG>().AgentSpeed;
            mode = "idle";
            Target = null;
        }
        if (mode != "leading" & Follower != null)
        { Debug.Log("hanging follower");
            releaseFollower(Follower);
         
        }
            //check if vehicle is stuck
            //Debug.Log(stuck(grabber, Agent));

            //check if object is selected and set selected status flag
            //Debug.Log("I'm attached to " + gameObject.name);

        if (arrived(Agent) == true)
        {   
            if (mode != "idle")
            {
                mode = "idle";
           
            }
            if (mode == "idle") {
                Agent.destination = Agent.transform.position; 
                 }
            
        }
        // if the vehicle is flipped set its mode
        if (flipped(grabber))
            {
            mode = "flipped";
            

        }
        // if mode is flipped but vehicle is not flipped set mode to idle
        if (mode == "flipped" && !flipped(grabber))
        {   
            
            Debug.Log("unflipped");
            mode = "idle";
        }
        drive();

    }
    void drive()
    {


        Debug.DrawRay(grabber.transform.position, grabber.transform.forward * 10, Color.blue);
        //steering calculations
        steeringAngle = Vector3.SignedAngle(
            (-new Vector3(grabber.transform.position.x, 0, grabber.transform.position.z) +
            new Vector3(Agent.transform.position.x, 0, Agent.transform.position.z) +
            Agent.transform.forward * Agent.velocity.magnitude),
            new Vector3(grabber.transform.forward.x, 0, grabber.transform.forward.z),
            Vector3.up);

        //Debug.DrawRay(grabber.transform.position, new Vector3(0, 0, steeringAngle), Color.green);
        //Debug.Log(steeringAngle);

        //pid calculations
        //p is forward backward distance 
        float fbd = Vector3.Dot(
            (grabber.transform.position - new Vector3(0, grabber.transform.position.y, 0)) - (Agent.transform.position - new Vector3(0, Agent.transform.position.y, 0)),
            grabber.transform.forward
            );

        // old p simple linear distance on ground plane
        float dist = ((grabber.transform.position - new Vector3(0, grabber.transform.position.y, 0)) - (Agent.transform.position - new Vector3(0, Agent.transform.position.y, 0))).magnitude;
        //if target is infront of grabber drive forwards else drive backwards
        if (fbd < 0)
        {
            p = -dist;
        }
        else
        {
            p = dist;
        }
        if (dist < 1 & Agent.desiredVelocity.magnitude < 0.1)
        {
            p = 0;
            dist = 0;
            i = 0;
            iold = 0;
        }
        //stop agent running away 
        if (stuck(grabber, Agent))
        {
            if (stucktimer < 0)
            {
                //is the agent infront or behind the grabber
                Vector3 relLoc = grabber.transform.position - Agent.transform.position;
                float dir = Vector3.Dot(relLoc, grabber.transform.forward);
                dir = -dir / Mathf.Abs(dir);

                Agent.transform.position = grabber.transform.position - 10 * dir * grabber.transform.forward;
                stucktimer = 2f;
                iold = 0;//reset i term to prevent hung over i term wind up
            }
            else
            {
                stucktimer -= Time.deltaTime;
            }
        }
        else
        {
            stucktimer = 2f;
        }
        if (dist > Vector3.Dot((Agent.transform.position - grabber.transform.position).normalized, grabber.transform.forward) * 5 && dist > 2)
        //if the grabber agent distance is greater than  5* the component of the angent-grabber distance in the grabber forward direction and greater than 1 stop the agent moving
        //this allows time for the grabber to make tight turns without the agent getting too far away.
        { //Debug.Log("agent speed = slow, mode 1");
            //if agent and grabber ar traveling in opposite directions 
            Agent.speed = 0;//AgentSlowSpeed;
        }
        else if (Vector3.Dot(Agent.desiredVelocity.normalized, grabber.transform.forward) < 0.0 && dist > 2)
        {
            //Debug.Log("agent speed = slow, mode 2");
            Agent.speed = 0;// AgentSlowSpeed; 
        }
        //else if (Vector3.Dot(Agent.desiredVelocity.normalized, grabber.transform.forward) > 0.8)
        //{
        //    //if the grabber is traveling in the right direction by luck let it keep going and put the agent on top of it
        //    Debug.Log("im buggering up the initialisation");
        //    Agent.Warp(grabber.transform.position);
        //    Agent.speed = AgentSpeed
        //}   


        else
        {
            //Debug.Log("agent speed > fast");
            Agent.speed = AgentSpeed;

        }
        //    if (steeringAngle > steeringSwitch| steeringAngle < -steeringSwitch)
        //{
        //    //p = -p;
        //    steeringAngle = -steeringAngle;
        //}
        //check agent is still on nav mesh
        if (!Agent.isOnNavMesh)
        {
            Agent.Warp(grabber.transform.position);
            Debug.Log("warping to navmesh");
        }
        steeringAngle = -steeringAngle;
        steeringAngle = steeringAngle - (steeringAngle + SAold) * SteeringD;
        SAold = steeringAngle;
        i = iold + p;// -dist;

        d = p - pold;

        pold = p;
        iold = i;
        iold = Mathf.Clamp(iold, -200, 200);
        dold = d;
        PEffect = -p * P + dist * P / 10;
        IEffect = -i * I;
        DEffect = -d * D;

        throttle = PEffect + IEffect + DEffect;
        if (throttle < 0 & grabber.transform.root.GetComponent<Rigidbody>().velocity.sqrMagnitude < 1)
        {
            steeringAngle = -steeringAngle;
        }
        //apply calculated values to wheels


        Brakes = false;
        //if traveling fast and making a tight turn apply brakes and reduce steering angle, gives smoother turns
        float vmag = grabber.transform.root.GetComponent<Rigidbody>().velocity.magnitude;
        if (vmag > (10 / (Mathf.Abs(steeringAngle))) +1)
        {
            Brakes = true;
            steeringAngle = steeringAngle / (vmag*10);
            throttle = 0;
        }
        //if trying to slow down apply brakes
        if (Vector3.Dot(grabber.transform.root.GetComponent<Rigidbody>().velocity, Agent.transform.position- grabber.transform.position)<0 && throttle< 0 &&fbd <0)//body moving , trottle is negative and forward/back distance to targetis negative
        {
            Brakes = true;
        }
        if (Vector3.Dot(grabber.transform.root.GetComponent<Rigidbody>().velocity, Agent.transform.position - grabber.transform.position) < 0  && throttle > 0 && fbd >0)
        {
            Brakes = true;
        }
        //Debug.Log(motor);
        if (mode == "flipped")
        {
            steeringAngle = 0;
            throttle = 0;
            Brakes = false;

        }
        float motor = Mathf.Clamp( throttle, -maxMotorTorque, maxMotorTorque); //Input.GetAxis("Vertical");
        float steering = Mathf.Clamp(steeringAngle, -maxSteeringAngle, maxSteeringAngle); //Input.GetAxis("Horizontal");
        foreach (AxleInfo axleInfo in axleInfos)
        {   if (Brakes)
            {
                axleInfo.leftWheel.brakeTorque = 100; //Mathf.Abs(throttle);
                axleInfo.rightWheel.brakeTorque = 100;//Mathf.Abs(throttle);
            }
            if(!Brakes)
            {
                axleInfo.leftWheel.brakeTorque = 0;
                axleInfo.rightWheel.brakeTorque = 0;
            }
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.reverseSteering)
            {
                axleInfo.leftWheel.steerAngle = -axleInfo.leftWheel.steerAngle;
                axleInfo.rightWheel.steerAngle = -axleInfo.rightWheel.steerAngle;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
        }
    }
  

    public bool arrived(NavMeshAgent Agent) {
        if (mode !="following")
        if (!Agent.pathPending&&Agent.isOnNavMesh)

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
   
    
    public List<GameObject> GetTaggedAndVisible(string tag)
    {
        List<GameObject> objs = new List<GameObject>();
        foreach (GameObject w in visible)
        {
            //Debug.Log(w.tag);
            //Debug.Log(tag);
            if ( w.tag == tag){
                
                objs.Add(w);
            }

        }
        return objs;
    }
    public List<GameObject> GetTaggedFromList(string tag, List<GameObject> L)
    {
        List<GameObject> objs = new List<GameObject>();
        foreach (GameObject w in L)
        {
            //Debug.Log(w.tag);
            //Debug.Log(tag);
            if (w.tag == tag)
            {

                objs.Add(w);
            }

        }
        return objs;
    }
    public List<GameObject> GetVisible(float visDist)
    {
        List<GameObject> objs = new List<GameObject>();

        Collider[] nearCols = Physics.OverlapSphere(grabber.transform.position, visDist);
        foreach (Collider w2 in nearCols)

        {
            if( w2.tag != "Terrain"){
                objs.Add(w2.transform.root.gameObject);
            }

        };

        return objs;
    }
    public GameObject NearestInList(NavMeshAgent Agent, List<GameObject>  Targets )
    {
        GameObject nearest;
        List<GameObject>  Collectables =Targets;
        nearest = null;
        foreach (GameObject w in Collectables)
        {   if (w == this.transform.root.gameObject) {
                Debug.Log("at least we are trying");
                continue; 
            }

            else if (nearest == null)
            {
                nearest = w;
            }

            else if (
                Vector3.Distance(grabber.transform.position, w.transform.position) <
                Vector3.Distance(grabber.transform.position, nearest.transform.position)
                )
            {

                nearest = w;

            }
        }
        return (nearest);
    }
    public bool stuck(GameObject vehicle, NavMeshAgent agent)
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
    public bool flipped(GameObject vehicle)
    {
        if
           (Vector3.Dot(vehicle.transform.up, Vector3.down) > -0.20)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void releaseFollower(GameObject follower)
    {
        Debug.Log("releasing follower");

        follower.transform.parent.GetComponent<navigate>().Target = null;
        follower.transform.parent.GetComponent<navigate>().mode="commanded";
        
        //follower.transform.parent.GetComponent<navigate>().Team = Team;
        //setTeam(follower.transform.parent);
        this.Follower = null;
        AgentSpeed = this.GetComponent<VehCFG>().AgentSpeed;
    }
    public void addFollowerToTeam(GameObject follower)
    {
        releaseFollower(follower);
        setTeam(follower.transform);
    }
    public void setTeam(Transform newMember)
    {
        //navigate nmnav = newMember.GetComponent<navigate>();
        Debug.Log("adding new member to team");
        Debug.Log(Team);
        //nmnav.Team = Team;
        Team.GetComponent<TeamHandler>().InitialiseVehicle(newMember.gameObject, Base, Home);
        // nmnav.Base = Base;
        //nmnav.Home = Home;
    }
}
