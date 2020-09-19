using UnityEngine;
using System.Collections;
using System.Collections.Generic;






public class trackdrive: MonoBehaviour
{
    public List<trackInfo> trackInfos; // the information about each individual track
    
    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    public HingeJoint boom1;
    public HingeJoint boom2;
    public HingeJoint hull;
    public Rigidbody trackCarrier;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pick Up"))
        {
       
            //     SetCountText();
            // other.gameObject.SetActive(false);
            //Destroy(other.gameObject);
        }
           if (other.gameObject.tag == "wall")
        {

            
            //Debug.Log("drive hit wall");
        }
    }


    public void FixedUpdate()
    {   //Drive "tracked" vehicle
        foreach (trackInfo track in trackInfos)
        {
            foreach (WheelCollider wheel in track.wheels)
            {
                if (track.left)
                {
                    wheel.motorTorque = maxMotorTorque * (Input.GetAxis("Vertical")/2 + Input.GetAxis("Horizontal"));
                    Debug.Log(wheel.motorTorque);
                    
                }
                if (!track.left)
                {
                    wheel.motorTorque = maxMotorTorque * (Input.GetAxis("Vertical")/2 - Input.GetAxis("Horizontal"));
                }
                //lazy speedlimiter
                if (Vector3.Dot(trackCarrier.transform.forward.normalized, trackCarrier.velocity )>= 3)
                {
                        wheel.motorTorque = Mathf.Min(0f, wheel.motorTorque);

                }
                if (Vector3.Dot(trackCarrier.transform.forward.normalized, trackCarrier.velocity)<= -3)
                {
                    wheel.motorTorque = Mathf.Max(0f, wheel.motorTorque);

                }
            }

        }


            // control motion of boom arm
            // this seems like a long way arround but it works 
            // step 1 create motors 
            JointMotor b1motor = new JointMotor();
            JointMotor b2motor = new JointMotor();
            
            // step 2 assign values
        b1motor.force = 1000f;
        b2motor.force = 1000f;
        b1motor.targetVelocity = Input.GetAxis("boom1") * 250f;
        b2motor.targetVelocity = Input.GetAxis("boom2") * 250f;
                
        b1motor.freeSpin = false;
        b2motor.freeSpin = false;
            //step 3 assign motors to boom elements
        boom1.motor = b1motor;
        boom2.motor = b2motor;
           


        // to yaw main body here

        //foreach (trackInfo trackInfo in trackInfos)
        JointMotor turretMotor = new JointMotor();
        turretMotor.force = 1000f;
        turretMotor.targetVelocity = Input.GetAxis("yaw")*-45f;
       // Debug.Log(turretMotor.targetVelocity);
        turretMotor.freeSpin = false;
        hull.motor = turretMotor;

        //{ 

            
           
        //        axleInfo.leftWheel.motorTorque = motor;
        //        axleInfo.rightWheel.motorTorque = motor;
        //    }
        //}
    }


}


[System.Serializable]

public class trackInfo
{

    public List<WheelCollider> wheels;
    public bool left; // is this the left track?
  }
