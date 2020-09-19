using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerControler : MonoBehaviour {
    private Rigidbody rb;
    int count;
    public Text countText;
    public Text winText;

    public float speed;
    void Start()
    {
        count = 0;
        SetCountText();
        rb = GetComponent<Rigidbody>();
        winText.text = "";
    }
    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.AddForce(movement * speed);
    }
    private void OnTriggerEnter(Collider other)
    {   if (other.gameObject.CompareTag("Pick Up"))
        {
            count = count + 1;
            SetCountText();
            other.gameObject.SetActive(false);
        //Destroy(other.gameObject);
        }
    }
    void SetCountText() {
        countText.text = "Count: " + count.ToString();
        if (count >= 12) {
            ScreenGrab();
            winText.text = "you win";
            
        }
    }
    void ScreenGrab()
    {
        var directory = new System.IO.DirectoryInfo(Application.dataPath);
        var path = System.IO.Path.Combine(directory.Parent.FullName, string.Format("Screenshot_{0}.png", System.DateTime.Now.ToString("yyyy-MM-dd-H-mm-ss")));
        Debug.Log("Taking screenshot to " + path);
        ScreenCapture.CaptureScreenshot(path + "screenshot");

    }
}
