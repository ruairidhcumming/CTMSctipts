using UnityEngine;
using System.Collections;
using System.Collections.Generic;



/*public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; // is this wheel attached to motor?
    public bool steering; // does this wheel apply steer angle?
}*/

public class drive: MonoBehaviour
{
    public List<AxleInfo> axleInfos; // the information about each individual axle
    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    public float maxSteeringAngle; // maximum steer angle the wheel can have
    public float age;
    public float score;
    public float[,] Control1;
    public float[,] Control2;
    float ud;
    float lr;
    float[,] udlr;
    private void Start()
    {
        age = 0;
        score = 0;
        

       


    }
    public Vector3 FindClosestEnemy()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Pick Up");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 dir = new Vector3 (1, 0, 0);
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
                dir = diff.normalized;
            }
        }
        //Debug.DrawRay(position, dir*10, Color.green);
        return dir;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pick Up"))
        {
            score = score + 1;

            //     SetCountText();
            // other.gameObject.SetActive(false);
            //Destroy(other.gameObject);
        }
           if (other.gameObject.tag == "wall")
        {

            
            Debug.Log("drive hit wall");
        }
    }


    public void FixedUpdate()
    {
       
        Vector3 dir = FindClosestEnemy();
       


        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

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


}


