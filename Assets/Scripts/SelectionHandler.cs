using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionHandler : MonoBehaviour
{
    public List<GameObject> selected;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider Col)
    {
        Debug.Log("added object to selection "+(Col.gameObject.name));
        selected.Add(Col.gameObject);

    }
    void OnTriggerExit(Collider Col)
    {
        Debug.Log("removed object from selection "+(Col.gameObject.name));
        selected.Remove(Col.gameObject);
    }
}
