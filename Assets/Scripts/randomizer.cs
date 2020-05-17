using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomizer : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        RandomPos();
	}
    void RandomPos()
    {
        Vector3 pos;
        pos = new Vector3(Random.Range(-10, 10), 1, Random.Range(-10, 10));
        transform.position = pos;
    }
}
