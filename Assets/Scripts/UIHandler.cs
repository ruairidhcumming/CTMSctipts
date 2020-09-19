using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    GameObject topSelected;
    Transform Canvas;
    Transform Panel;
    // Start is called before the first frame update
    void Start()
    {
        Canvas = this.GetComponent<TeamHandler>().canvas;
        Panel = Canvas.Find("Panel");
    }

    // Update is called once per frame
    void Update()
    {

        //find ui components attached to selected game object and display them
        if ( this.GetComponent<TeamHandler>().selected.Count > 0)
        { 
            topSelected = this.GetComponent<TeamHandler>().selected[0];
            //Debug.Log(topSelected.name);
            Button[] buttons = topSelected.GetComponentsInChildren<Button>();

            foreach (Button b in buttons)
            {
               // Debug.Log(b.name);
                b.interactable = true ;
                //       newb.transform.parent = Panel;
                
            }
        }
    }
}
