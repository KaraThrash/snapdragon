using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovePanel : MonoBehaviour
{
    public GameObject panelInstance;

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
            panelInstance.transform.parent = null;

            Vector3 hoverPosition = anchor.position + anchorOffset;
            float dist = Vector3.Distance(hoverPosition, transform.position);

            transform.position = Vector3.MoveTowards(transform.position, hoverPosition, Time.deltaTime * speed * dist);

            Quaternion newRot = Quaternion.LookRotation(transform.position - observer.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * rotSpeed * (1 + (dist * 0.1f)));

        }
    }
}
