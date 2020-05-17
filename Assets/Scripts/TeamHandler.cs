using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamHandler : MonoBehaviour
{
    public List<GameObject> units = new List<GameObject>();
    public List<GameObject> buildings = new List<GameObject>();
    public List<GameObject> selected = new List<GameObject>();
    Vector3 select1 = new Vector3(-9999, 0, 0);
    Vector3 select2 = new Vector3(-9999, 0, 0);
    Vector3 ForwardFlat;
    Material SelectMaterial;
    GameObject cube;
    GameObject sphere1;
    GameObject sphere2;
    public GameObject car;
    public GameObject selector;
    int terrainMask = 1 << 8;
    // Start is called before the first frame update
    void Start()
    {
        SelectMaterial = Resources.Load("Selector", typeof(Material)) as Material;
        //instantiate first car
        // UnityEngine.Object prefab = Resources.Load("Assets/FunNGames 1/prefabs/Car1.prefab", typeof(GameObject));
        GameObject Base = GameObject.Find("Base");
        GameObject home = GameObject.Find("Base/Home");
        car = Instantiate(car,home.transform.position + new Vector3(0,2, 0), Quaternion.identity);
// Instantiate(Resource, base.transform.position + new Vector3(Random.Range(0, 100), 2000, Random.Range(0, 200)), Quaternion.identity);
        // GameObject car=
        //
        //
        //
        car.GetComponent<navigate>().Home = home.transform;
        car.GetComponent<navigate>().Base = Base.transform;
        units.Add(car);
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
            {   Debug.Log("Click Hit");

                //actions dependent on object clicked need to be handled here
                if (hit.transform)
                {

                    select1 = hit.point;
                    cube = Instantiate(selector);
                    //cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //sphere1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //sphere1.transform.position = select1;
                    //sphere2.transform.position = select1;
                    //sphere1.GetComponent<Renderer>().material = SelectMaterial;
                    //sphere2.GetComponent<Renderer>().material = SelectMaterial;
                    cube.GetComponent<Renderer>().material = SelectMaterial;
                    cube.GetComponent<Renderer>().shadowCastingMode= UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
        }
        if (Input.GetMouseButton(0))//should track while mouse is held
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000, terrainMask))
            {   Debug.Log("Hold Hit");
                Debug.Log(hit.transform.gameObject.tag);

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
                Debug.DrawRay(Camera.main.transform.position, ForwardFlat*10);

                cube.transform.position = (select2 + select1) / 2;
                cube.transform.rotation = Quaternion.LookRotation(ForwardFlat);
                cube.transform.localScale = new Vector3(Vector3.Dot((select2 - select1), Camera.main.transform.right), 100f, Vector3.Dot((select2 - select1), ForwardFlat));
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            selected = selectInCube(cube);
            Destroy(cube);

        }
            // if both select points are set draw box between them and select objects inside the box
            if (select1.x >-9999 & select2.x > -9999)
        {//find horizontal camera forward
            
                 if (Input.GetMouseButtonUp(0))//when mouse is released select units in box, destroy box, reset select points to 'null'
                 {
                    Debug.Log("Mouse Released");
                    select1 = new Vector3(-9999, 0, 0);
                    select2 = new Vector3(-9999, 0, 0);
                    
                 }
        }
    }
    List<GameObject> selectInCube(GameObject cube)
    {
        List<GameObject> inpt = cube.GetComponent<SelectionHandler>().selected;
        List<GameObject> otpt = new List<GameObject>();
        //select from units first
        foreach(GameObject obj in inpt)
        {
            foreach(GameObject unit in units)
            {
                if (unit == obj || obj.transform.IsChildOf(unit.transform))
                {
                    otpt.Add(unit);
                }
            }
           
        }
        return otpt;
    }
}
