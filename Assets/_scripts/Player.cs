using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using InputDevice = UnityEngine.XR.InputDevice;
using Qualcomm.Snapdragon.Spaces.Samples;
using Qualcomm.Snapdragon.Spaces;

public class Player : SampleController
{
  public Text LeftHandGestureName;
  public Text LeftHandGestureRatio;
  public Text LeftHandFlipRatio;
  public Text RightHandGestureName;
  public Text RightHandGestureRatio;
  public Text RightHandFlipRatio;

  public GameObject Mirror;
  public GameObject MirroredPlayer;
  public GameObject MirroredPlayerHead;
  public GameObject MirroredJointObject;


  private GameObject[] _leftMirroredHandJoints;
  private GameObject[] _rightMirroredHandJoints;
  private Transform _mainCameraTransform;
  private SpacesHandManager _spacesHandManager;

  public override void Start() {
      base.Start();
      _mainCameraTransform = Camera.main.transform;
      _spacesHandManager = FindObjectOfType<SpacesHandManager>();
      _spacesHandManager.handsChanged += UpdateGesturesUI;

  }


  public override void Update() {
      base.Update();
      // UpdateMirroredPlayer();
  }

  private void UpdateGesturesUI(SpacesHandsChangedEventArgs args) {
      foreach (var hand in args.updated) {
          var gestureNameTextField = hand.IsLeft ? LeftHandGestureName : RightHandGestureName;
          var gestureRatioTextField = hand.IsLeft ? LeftHandGestureRatio : RightHandGestureRatio;
          var flipRatioTextField = hand.IsLeft ? LeftHandFlipRatio : RightHandFlipRatio;

          // gestureNameTextField.text = Enum.GetName(typeof(SpacesHand.GestureType), hand.CurrentGesture.Type);
          gestureRatioTextField.text = (int) (hand.CurrentGesture.GestureRatio * 100f) + " %";
          // flipRatioTextField.text = hand.CurrentGesture.FlipRatio.ToString("0.00");
          flipRatioTextField.text = hand.transform.position.ToString();

          MirroredPlayer.transform.position = GetMirroredPosition(_mainCameraTransform.transform.position);

          var reflectedForward = Vector3.Reflect(hand.transform.rotation * Vector3.forward, Mirror.transform.forward);
          var reflectedUp = Vector3.Reflect(hand.transform.rotation * Vector3.up, Mirror.transform.forward);
          MirroredPlayerHead.transform.rotation = Quaternion.LookRotation(reflectedForward, reflectedUp);
      }

      foreach (var hand in args.removed) {
          var gestureNameTextField = hand.IsLeft ? LeftHandGestureName : RightHandGestureName;
          var gestureRatioTextField = hand.IsLeft ? LeftHandGestureRatio : RightHandGestureRatio;
          var flipRatioTextField = hand.IsLeft ? LeftHandFlipRatio : RightHandFlipRatio;

          gestureNameTextField.text = "-";
          gestureRatioTextField.text = "-";
          flipRatioTextField.text = "-";
      }
  }

  private void UpdateMirroredPlayer() {
      MirroredPlayer.transform.position = GetMirroredPosition(_mainCameraTransform.transform.position);

      var reflectedForward = Vector3.Reflect(_mainCameraTransform.transform.rotation * Vector3.forward, Mirror.transform.forward);
      var reflectedUp = Vector3.Reflect(_mainCameraTransform.transform.rotation * Vector3.up, Mirror.transform.forward);
      MirroredPlayerHead.transform.rotation = Quaternion.LookRotation(reflectedForward, reflectedUp);

      // UpdateMirroredHand(true);
      // UpdateMirroredHand(false);
  }

  // private void UpdateMirroredHand(bool leftHand) {
  //     var joints = leftHand ? _leftMirroredHandJoints : _rightMirroredHandJoints;
  //     for (var i = 0; i < _leftMirroredHandJoints.Length; i++) {
  //         var hand = leftHand ? _spacesHandManager.LeftHand : _spacesHandManager.RightHand;
  //         if (hand == null) {
  //             joints[i].SetActive(false);
  //             continue;
  //         }
  //         joints[i].SetActive(true);
  //         joints[i].transform.position = GetMirroredPosition(hand.Joints[i].Pose.position);
  //     }
  // }

  private Vector3 GetMirroredPosition(Vector3 positionToMirror) {
      /* Maths for reflection across a line can be found here: https://en.wikipedia.org/wiki/Reflection_(mathematics) */
      var mirrorNormal = Mirror.transform.forward;
      var mirrorPosition = Mirror.transform.position;
      /* Position to be reflected in a hyperplane through the origin. Therefore offset, the position by the mirror position. */
      var adjustedPosition = positionToMirror - mirrorPosition;
      var reflectedPosition = adjustedPosition - 2  * Vector3.Dot(adjustedPosition, mirrorNormal) / Vector3.Dot(mirrorNormal, mirrorNormal) * mirrorNormal;

      /* Offset the origin of the reflection again by the mirror position. */
      return mirrorPosition + reflectedPosition;
  }
}
