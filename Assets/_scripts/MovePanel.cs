using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovePanel : MonoBehaviour
{
    public GameObject panelInstance;
    public GameObject imageTracker;

    public Vector3 anchorOffset;
    public Transform anchor;
    public bool hover;
    public float speed;
    public float rotSpeed;
    public Transform observer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.isPressed)
        {
            Debug.Log("Pressed");
            //myObject.GetComponent<MyScript>().MyFunction();
            //Vector3 hoverPosition = imageTracker.GetComponent<ImageTrackingSampleController>();    //new Vector3 (0,0,2);
            Vector3 hoverPosition = new Vector3(0, 0, 2);
            float dist = Vector3.Distance(hoverPosition, transform.position);

            panelInstance.transform.position = Vector3.MoveTowards(panelInstance.transform.position, hoverPosition, Time.deltaTime * speed * dist);

            Quaternion newRot = Quaternion.LookRotation(panelInstance.transform.position - observer.position);
            panelInstance.transform.rotation = Quaternion.Slerp(panelInstance.transform.rotation, newRot, Time.deltaTime * rotSpeed * (1 + (dist * 0.1f)));

        }
    }
}
