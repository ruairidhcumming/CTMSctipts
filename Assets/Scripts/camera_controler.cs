using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_controler : MonoBehaviour
{
    float ScrollSpeed = 50f;
    float ScrollEdge = 0.01f;
     
    //private int HorizontalScroll = 1;
    //private int VerticalScroll = 1;
    //private int DiagonalScroll = 1;
     
    //float PanSpeed = 10f;
    float TurnSpeed = 30f;
    float PanRotateSpeed= 90f;
    Vector2 ZoomRange = new Vector2(0.5f,100f);
    Vector2 FollowZoomRange = new Vector2(-50, 50);

    float CurrentZoom = 10f;
    float ZoomZpeed = 1f;
    float ZoomRotation = 0.5f;
    Quaternion zoomtilt;
    float panrotation= 0f;
    private Vector3 vel;
    private Vector3 groundPos;
    private Vector3 InitRotation;
    private Vector3 FlatForward;
    private float y;
    private float x;
    int terrainMask = 1 << 8;
    public string mode = "fly"; //camera modes "fly" move arround manually, "follow" follow selected object 
    public GameObject target;// target for follow mode
    void Start()
    {
        //Instantiate(Arrow, Vector3.zero, Quaternion.identity);

        //InitPos = transform.position;
        InitRotation = transform.eulerAngles;


    }

    void Update()
    {
        FlatForward = new Vector3(transform.forward.x, 0, transform.forward.z);
        if (mode == "follow")
            
        {   //ZOOM IN/OUT follow mode
            if (Input.GetKey("d") || Input.mousePosition.x >= Screen.width * (1 - ScrollEdge)
                || Input.GetKey("a") || Input.mousePosition.x <= Screen.width * ScrollEdge
                || Input.GetKey("w") || Input.mousePosition.y >= Screen.height * (1 - ScrollEdge)
                || Input.GetKey("s") || Input.mousePosition.y <= Screen.height * ScrollEdge
                || target == null)
            {
                mode = "fly";
                //reset camera to look forwards with "up" alligned to world
                transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
                return;
            }
            
            if (target.GetComponent<Rigidbody>() != null)
            {
                vel =target.GetComponent<Rigidbody>().velocity.normalized;
                Debug.DrawRay(target.transform.position, vel * 1, Color.green);
                if (vel.magnitude < 0.1)
                {
                 vel = target.transform.InverseTransformDirection(target.transform.forward);

                    Debug.DrawRay(target.transform.position, vel * 1, Color.red);
                }
            }
            else
            {
                 vel = target.transform.InverseTransformDirection(target.transform.forward);

            }
            Debug.Log(target);
            Debug.Log(target.transform.forward);

            CurrentZoom -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 1000 * ZoomZpeed;
            //rotation geometry is all screwed up maybe its sign convention, maybe its maybelene
            CurrentZoom = Mathf.Clamp(CurrentZoom, FollowZoomRange.x, FollowZoomRange.y);
            zoomtilt = Quaternion.Euler(new Vector3(0,   0, -panrotation));
            zoomtilt = zoomtilt * Quaternion.Euler(new Vector3((transform.eulerAngles.x - (InitRotation.x + CurrentZoom * ZoomRotation)) * -0.5f, 0, 0));
            //transform.position = transform.position - new Vector3(0, (transform.position.y - (InitPos.y + CurrentZoom)) * 0.1f, 0);
            // transform.eulerAngles = transform.eulerAngles - new Vector3((transform.eulerAngles.x - (InitRotation.x + CurrentZoom * ZoomRotation)) * 0.1f, 0, 0);
            //set position to follow "target"
            //Debug.DrawRay(target.transform.position, vel * 5, Color.red);
            //Debug.DrawRay(target.transform.position, Vector3.up * 2, Color.green);
            transform.position = 
                target.transform.position  
                + (-(vel * 5))
                + (Vector3.up * 2) ;
                
                //- new Vector3(0, (target.transform.position.y - (InitPos.y + CurrentZoom)) * 0.1f, 0)));

            if (Input.GetKey("mouse 2"))
            {
                //(Input.mousePosition.x - Screen.width * 0.5)/(Screen.width * 0.5)

                panrotation+=( Time.deltaTime * PanRotateSpeed * (Input.mousePosition.x - Screen.width * 0.5f) / (Screen.width * 0.5f));
                // transform.Rotate(Vector3.forward * Time.deltaTime * PanSpeed * (Input.mousePosition.y - Screen.height * 0.5f) / (Screen.height * 0.5f), Space.World);

            }

            transform.rotation = Quaternion.LookRotation(vel, Vector3.up) * zoomtilt* Quaternion.Euler(-40, 0, 0) ;


            //if we touch the screen edges or give WASD comand stop following and go back to fly mode


        }
        if (mode == "fly")
        {   

            
            //check if camera is bellow ground level and move
            //Debug.Log("Click");

            RaycastHit hit;
            Ray ray = new Ray(Camera.main.transform.position, Vector3.up);
            Debug.DrawRay(Camera.main.transform.position-Vector3.up, Vector3.up, Color.red);
            if (Physics.Raycast(ray, out hit, 1000,terrainMask))
            { //Debug.Log("camera under ground");
                Camera.main.transform.position = hit.point+Vector3.up*0.1f;
            }
            ray = new Ray(Camera.main.transform.position+ Vector3.up, Vector3.up * -1f);
            if (Physics.Raycast(ray, out hit, 1000, terrainMask))
            {
                groundPos = hit.point;
                //Debug.Log("camera above ground");
                if ((Camera.main.transform.position - hit.point).magnitude<1.5)
                {
                    Debug.DrawRay(Camera.main.transform.position, Vector3.up * -1f, Color.red);
                    Camera.main.transform.position += Vector3.up * 0.1f;
                }
            }

            //PAN
            //if (Input.GetKey("mouse 2"))
            //{
            //    //(Input.mousePosition.x - Screen.width * 0.5)/(Screen.width * 0.5)

            //    transform.Translate(Vector3.right * Time.deltaTime * PanSpeed * (Input.mousePosition.x - Screen.width * 0.5f) / (Screen.width * 0.5f), Space.World);
            //    transform.Translate(Vector3.forward * Time.deltaTime * PanSpeed * (Input.mousePosition.y - Screen.height * 0.5f) / (Screen.height * 0.5f), Space.World);

            //}
            //else
           // {
                if (Input.GetKey("d") || Input.mousePosition.x >= Screen.width * (1 - ScrollEdge))
                {
                    transform.Translate(transform.right * Time.deltaTime * ScrollSpeed, Space.World);
                }
                else if (Input.GetKey("a") || Input.mousePosition.x <= Screen.width * ScrollEdge)
                {
                    transform.Translate(transform.right * Time.deltaTime * -ScrollSpeed, Space.World);
                }

                if (Input.GetKey("w") || Input.mousePosition.y >= Screen.height * (1 - ScrollEdge))
                {
                    transform.Translate(FlatForward * Time.deltaTime * ScrollSpeed, Space.World);
                }
                else if (Input.GetKey("s") || Input.mousePosition.y <= Screen.height * ScrollEdge)
                {
                    transform.Translate(FlatForward * Time.deltaTime * -ScrollSpeed, Space.World);
                }
            //}
        
            //ZOOM IN/OUT fly mode

            CurrentZoom -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 1000 * ZoomZpeed;

            CurrentZoom = Mathf.Clamp(CurrentZoom, ZoomRange.x, ZoomRange.y);

            transform.position = transform.position - new Vector3(0, (transform.position.y - (groundPos.y + CurrentZoom)) * 0.1f, 0);
            transform.eulerAngles = transform.eulerAngles - new Vector3((transform.eulerAngles.x - (InitRotation.x + CurrentZoom * ZoomRotation)) * 0.1f, 0, 0);
            //rotate view

            if (Input.GetKey("mouse 2"))
            {
                //(Input.mousePosition.x - Screen.width * 0.5)/(Screen.width * 0.5)

                transform.Rotate(Vector3.up * Time.deltaTime * TurnSpeed * (Input.mousePosition.x - Screen.width * 0.5f) / (Screen.width * 0.5f), Space.World);
                // transform.Rotate(Vector3.forward * Time.deltaTime * PanSpeed * (Input.mousePosition.y - Screen.height * 0.5f) / (Screen.height * 0.5f), Space.World);

            }
        }
       
    }

}
