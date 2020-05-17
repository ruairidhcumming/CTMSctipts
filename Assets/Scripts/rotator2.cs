using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotator2 : MonoBehaviour {

    void Update()
    {
        transform.Rotate(new Vector3(15f, 30f, 45f) * Time.deltaTime);
    }
}
