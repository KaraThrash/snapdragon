using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Qualcomm.Snapdragon.Spaces.Samples;
public class sandbox : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform textObj;
    [SerializeField] private float distanceToDisplayText;
    [SerializeField] private float speed;
    [SerializeField] private float rotSpeed;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        LookAtPlayer();
    }

    public void LookAtPlayer()
    {
      if(playerCamera == null || textObj == null){return;}



      Vector3 cameraForward = playerCamera.position + (playerCamera.forward * distanceToDisplayText);
      float dist = Vector3.Distance(cameraForward,textObj.position);

      textObj.position = Vector3.MoveTowards(textObj.position, cameraForward, Time.deltaTime * speed * dist);

      Quaternion newRot = Quaternion.LookRotation(textObj.position - playerCamera.position);
      textObj.rotation = Quaternion.Slerp(textObj.rotation, newRot, Time.deltaTime * rotSpeed * (1 + (dist * 0.1f)) );

      newRot = Quaternion.LookRotation(textObj.position - (playerCamera.position + (playerCamera.right * 0.2f)));
      textObj.GetChild(0).rotation = Quaternion.Slerp(textObj.rotation, newRot, Time.deltaTime * rotSpeed * (1 + (dist * 0.1f)) );
      newRot = Quaternion.LookRotation(textObj.position - (playerCamera.position - (playerCamera.right * 0.2f)));
      textObj.GetChild(1).rotation = Quaternion.Slerp(textObj.GetChild(1).rotation, newRot, Time.deltaTime * rotSpeed * (1 + (dist * 0.1f)) );


    }
}
