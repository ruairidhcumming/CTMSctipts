using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TeamHandler : MonoBehaviour
{   //LISTS OF UNITS BUILDINGS ETC
    public List<GameObject> units = new List<GameObject>();
    public List<GameObject> buildings = new List<GameObject>();
    public List<GameObject> selected = new List<GameObject>();

    // POINTS FOR SELECT BOX INTERACTIONS DEFAULTING TO FAR AWAY
    Vector3 select1 = new Vector3(-9999, 0, 0);
    Vector3 select2 = new Vector3(-9999, 0, 0);
    Vector3 ForwardFlat;
    //MATERIALS FOR HIGHLIGHTING SELECTED OBJECTS
    Material SelectMaterial;
    Material HighlightMaterial;
    //GAME OBJECTS FOR SELECTION MECANIC
    GameObject cube;
    GameObject sphere1;
    GameObject sphere2;
    public GameObject selector;
    //LIST FOR MANAGING RESOURCES
    public Dictionary<string, float> TeamResources = new Dictionary<string, float>();
    //UI COMPONENTS
    public GameObject UIOverlay;
    public Transform canvas;
    GameObject UIObject; //game object whose UI is currently shown
    string txt;
    Text textbox;
    //game objects to create 
    public GameObject car;
    //masks for raycast interactions
    int terrainMask = 1 << 8;

    // Start is called before the first frame update
    void Start()
    {
        SelectMaterial = Resources.Load("Selector", typeof(Material)) as Material;
        //HighlightMaterial = Resources.Load("outline", typeof(Material)) as Material;
        //instantiate first car
        // UnityEngine.Object prefab = Resources.Load("Assets/FunNGames 1/prefabs/Car1.prefab", typeof(GameObject));
        GameObject Base = GameObject.Find("Base");
        GameObject home = GameObject.Find("Base/Home");
        car = Instantiate(car, home.transform.position + new Vector3(0, 2, 0), Quaternion.identity);
        // Instantiate(Resource, base.transform.position + new Vector3(Random.Range(0, 100), 2000, Random.Range(0, 200)), Quaternion.identity);
        // GameObject car=
        //
        //
        //
        car.GetComponent<navigate>().Home = home.transform;
        car.GetComponent<navigate>().Base = Base.transform;
        car.GetComponent<navigate>().Team = gameObject.transform;
        units.Add(car);
        buildings.Add(Base);
        canvas = transform.Find("Canvas");
        textbox = canvas.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {


        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Click");
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000, terrainMask))
            {  // Debug.Log("Click Hit");

                //actions dependent on object clicked need to be handled here
                if (hit.transform)
                {

                    select1 = hit.point;
                    select2 = hit.point;
                    cube = Instantiate(selector);
                    //cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //sphere1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //sphere1.transform.position = select1;
                    //sphere2.transform.position = select1;
                    //sphere1.GetComponent<Renderer>().material = SelectMaterial;
                    //sphere2.GetComponent<Renderer>().material = SelectMaterial;
                    cube.GetComponent<Renderer>().material = SelectMaterial;
                    cube.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
        }

        if (Input.GetMouseButton(0))//should track while mouse is held
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000, terrainMask))
            {   //Debug.Log("Hold Hit");
                // Debug.Log(hit.transform.gameObject.tag);

                if (hit.transform.gameObject.tag == "Terrain")
                {
                    select2 = hit.point;
                    // Debug.Log(select2.ToString());
                    //Debug.Log("moving sphere2");
                    //sphere2.transform.position = select2;
                }
                ForwardFlat = Camera.main.transform.forward;
                ForwardFlat.y = 0f;
                ForwardFlat = ForwardFlat.normalized;
                // cube, rotate and scale
                Debug.DrawRay(Camera.main.transform.position, ForwardFlat * 10);

                cube.transform.position = (select2 + select1) / 2;
                cube.transform.rotation = Quaternion.LookRotation(ForwardFlat);
                cube.transform.localScale = new Vector3(Vector3.Dot((select2 - select1), Camera.main.transform.right), 100f, Vector3.Dot((select2 - select1), ForwardFlat));
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            selected = selectInCube(cube);
            Debug.Log("destroy selector cube");
            Destroy(cube);

        }
        // if both select points are set draw box between them and select objects inside the box
        if (select1.x > -9999 & select2.x > -9999)
        {//find horizontal camera forward

            if (Input.GetMouseButtonUp(0))//when mouse is released select units in box, destroy box, reset select points to 'null'
            {
                //Debug.Log("Mouse Released");
                select1 = new Vector3(-9999, 0, 0);
                select2 = new Vector3(-9999, 0, 0);

            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            //Debug.Log("Click");
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000, terrainMask))
            {
                // Debug.Log("Click Hit");

                //actions dependent on object clicked need to be handled here
                if (hit.transform)
                {

                    foreach (GameObject obj in selected)
                    { if (obj.GetComponent<navigate>() != null)
                        {
                            obj.GetComponent<navigate>().mode = "commanded";
                            obj.GetComponent<navigate>().Agent.SetDestination(hit.point);
                        }
                    }


                }
            }
        }
        //set score displays
        //Get resources from all buildings and reset building resources 
        foreach (GameObject b in buildings)
        {
            Dictionary<string, float> childList = b.GetComponent<baseControlScript>().Resources;
            if (childList != null) {

                foreach (KeyValuePair<string, float> res in childList)
                {
                    if (TeamResources.ContainsKey(res.Key))
                    {
                        TeamResources[res.Key] += res.Value;
                        Debug.Log("resource increased");

                    }
                    else
                    {
                        TeamResources.Add(res.Key, res.Value);
                        Debug.Log("added new resource");
                    }

                }
                //clear list from child buildings to prevent double counting
                b.GetComponent<baseControlScript>().Resources.Clear();
            }

        }
        //update text with values from resuoures dict
        txt = "";
        foreach (KeyValuePair<string, float> res in TeamResources)
        {
            txt = txt + res.Key + " : " + res.Value.ToString() + "\n";
        }
        textbox.text = txt;

        //check which object is currently selected and draw UI

    }

    List<GameObject> selectInCube(GameObject cube)
    {
        List<GameObject> inpt = cube.GetComponent<SelectionHandler>().selected;
        List<GameObject> otpt = new List<GameObject>();
        //select from units first
        foreach (GameObject obj in inpt)
        {
            foreach (GameObject unit in units)
            {
                if (unit == obj || obj.transform.IsChildOf(unit.transform))
                {
                    otpt.Add(unit);
                }
            }

        }
        if (otpt.Count == 0)
        {
            foreach (GameObject obj in inpt)
            {
                foreach (GameObject unit in buildings)
                {
                    if (unit == obj || obj.transform.IsChildOf(unit.transform))
                    {
                        otpt.Add(unit);
                    }
                }
            }
        }
        return otpt;
        
    }
    public void dropWrapper(GameObject dropable)
    {
        //do cost and tech checks here 
        Debug.Log(dropable);
        StartCoroutine(pickDropsite(dropable));
  
    }
    public IEnumerator pickDropsite(GameObject dropable)

     {
     Debug.Log("Dropsite running");
     while(true)
     {
         if(Input.GetMouseButtonDown(0))
         {
             RaycastHit hit = new RaycastHit();
             if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
             {
                
                Debug.Log("new object instanitated ");
                Instantiate(dropable, hit.point + Vector3.up* 20, new Quaternion(0f,0f,0f,0f));
                yield break;
             }
         }
         yield return null;
     }
     //not here yield return null;
    }

}
