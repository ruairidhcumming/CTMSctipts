using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class myVehCFG : MonoBehaviour
{
    public float sightRadius = 20;

    public float hp = 100;
    public float str = 10;
    public Material HighlightMaterial;
    public Material NormalMaterial;
    // Start is called before the first frame update
    void Start()
    {
        // HighlightMaterial = Resources.Load("outline", typeof(Material)) as Material;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
