using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
  private bool alternate;
  public float swipeDistance = 0.5f;
  public float positionScale = 0.1f;

  public GameObject prefab_hoverPanel;
  public TextHoverPanel hoverPanel;
  public List<TextHoverPanel> activePanels;

  public Transform parent_panels;
  public Transform targetPerson;

  public Text debugText;

  public UnityEvent swipeRight;
  public UnityEvent swipeLeft;
  public UnityEvent swipeUp;
  public UnityEvent swipeDown;

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


  public bool lh_isVisible;
  public bool rh_isVisible;

  public Vector3 lh_StartPos;
  public Vector3 rh_StartPos;






  public override void Start() {
      base.Start();
      _mainCameraTransform = Camera.main.transform;
      _spacesHandManager = FindObjectOfType<SpacesHandManager>();
      _spacesHandManager.handsChanged += UpdateGesturesUI;

  }


  public override void Update() {
      base.Update();

      if(Keyboard.current.aKey.wasPressedThisFrame )
      {Swipe_Left();  }
      if(Keyboard.current.sKey.wasPressedThisFrame )
      {Swipe_Right();  }
      if(Keyboard.current.dKey.wasPressedThisFrame )
      {Swipe_Up();  }
      if(Keyboard.current.fKey.wasPressedThisFrame )
      {Swipe_Down();  }

  }


public TextHoverPanel GetPanel()
{
  TextHoverPanel newHover = null;
  if(parent_panels == null){parent_panels = new GameObject().transform;}

  if(parent_panels.childCount == 0 || parent_panels.GetChild(0).gameObject.activeSelf)
  {
      newHover = Instantiate(prefab_hoverPanel).GetComponent<TextHoverPanel>();

  }else{newHover = parent_panels.GetChild(0).GetComponent<TextHoverPanel>();}

    newHover.transform.parent = null;
    newHover.transform.parent = parent_panels;
    activePanels.Add(newHover);
    newHover.anchor = targetPerson;
    newHover.gameObject.SetActive(true);
  return newHover;
}

public void UpdateDebugText(string _text)
{
  if(debugText)
  {debugText.text = _text + debugText.text ;}

}

public void UpdateHandVisible(SpacesHand hand)
{

    if(hand.IsLeft)
    {
      if(lh_isVisible == false)
      {
        UpdateDebugText("left hand visible \n");

        lh_StartPos = hand.transform.position;
        lh_isVisible = true;
        if(hoverPanel)
        {
          hoverPanel.SetColor(0);

        }
      }
    }
    else
    {
      if(rh_isVisible == false)
      {
        UpdateDebugText("right hand visible \n");
          rh_StartPos = hand.transform.position;
          rh_isVisible = true;
      }
    }

}

public void HandExit(SpacesHand hand)
{

    if(hand.IsLeft)
    {
      if(lh_isVisible == true)
      {
          DetectSwipe(hand,lh_StartPos);

          if(hoverPanel)
          {
          //  hoverPanel.SetColor(1);

          }
        if (Vector3.Distance(hand.transform.position , lh_StartPos) > swipeDistance)
        {
          //ShowPanel();

        }
        lh_isVisible = false;
      }
    }
    else
    {
      if(rh_isVisible == true)
      {
        DetectSwipe(hand,rh_StartPos);

          rh_isVisible = false;
          if (Vector3.Distance(hand.transform.position , rh_StartPos) > swipeDistance)
          {
          //  HidePanel();

          }

      }
    }

}

public void DetectSwipe(SpacesHand _hand,Vector3 _startPos)
{
  float dist = Vector3.Distance(_hand.transform.position , _startPos);
  float hort = _hand.transform.position.x - _startPos.x;
  float vert = _hand.transform.position.y - _startPos.y;
  float depth = _hand.transform.position.z - _startPos.z;

  if (dist < swipeDistance)
  {return;}

  if(Mathf.Abs(vert) >  Mathf.Abs(hort))
  {
    if(vert <= 0)
    {
     Swipe_Down();
    }else
    {
      Swipe_Up();

    }

  }
  else
  {

    if(hort <= 0)
    {
     Swipe_Left();
    }else
    {
      Swipe_Right();

    }
  }


}

public void Swipe_Up()
{
  swipeUp.Invoke();
  UpdateDebugText("swipe up" + '\n');
  if(activePanels != null && activePanels.Count > 1)
  {
    TextHoverPanel panel = activePanels[activePanels.Count - 1];
    activePanels.RemoveAt(activePanels.Count - 1);
    activePanels.Insert(0,panel);
    panel.anchorOffset = new Vector3(panel.anchorOffset.x,Mathf.Abs(panel.anchorOffset.y),panel.anchorOffset.z);

  }
}
public void Swipe_Down()
{
  swipeDown.Invoke();
  UpdateDebugText("swipe down" + '\n');
  if(activePanels != null && activePanels.Count > 1)
  {
    TextHoverPanel panel = activePanels[0];
    activePanels.RemoveAt(0);
    panel.anchorOffset = activePanels[activePanels.Count - 1].anchorOffset;
    panel.anchorOffset = new Vector3(panel.anchorOffset.x,Mathf.Abs(panel.anchorOffset.y) * -1,panel.anchorOffset.z);
    activePanels.Add(panel);

  }
}

public void Swipe_Left()
{
  swipeLeft.Invoke();
  UpdateDebugText("swipe left" + '\n');
  if(activePanels != null && activePanels.Count > 0)
  {
    activePanels[activePanels.Count - 1].hover = false;
    activePanels[activePanels.Count - 1].anchorOffset = Vector3.zero;
    activePanels[activePanels.Count - 1].gameObject.SetActive(false);
    activePanels.RemoveAt(activePanels.Count - 1);
  }

}
public void Swipe_Right()
{
  swipeRight.Invoke();
  UpdateDebugText("swipe right" + '\n');
  TextHoverPanel newPanel = GetPanel();
  newPanel.gameObject.SetActive(true);
  newPanel.hover = true;
  newPanel.observer = this.transform;
  newPanel.anchor = targetPerson;

  newPanel.anchorOffset  = new Vector3(0,0,0);
newPanel.NextStyle(activePanels.Count);

  if(alternate)
  {
    newPanel.SetRowInformation(0,activePanels.Count.ToString());

    newPanel.anchorOffset  += new Vector3(0,-(activePanels.Count + 1)* positionScale ,-(activePanels.Count + 1)* positionScale);
  }
  else
  {
      newPanel.SetRowInformation(0,"not " + activePanels.Count.ToString());
      newPanel.anchorOffset  += new Vector3(0,(activePanels.Count + 1) *  positionScale ,-(activePanels.Count + 1)* positionScale);
      newPanel.SetColor(0);

  }
  alternate = !alternate;
}


public void ShowPanel()
{
  UpdateDebugText("SHOW panel" + '\n');
  if(hoverPanel)
  {
    hoverPanel.gameObject.SetActive(true);

    hoverPanel.hover = true;
  }

}

public void HidePanel()
{

  UpdateDebugText("Hide Panel" + '\n');
  if(hoverPanel)
  {
      hoverPanel.hover = false;
      hoverPanel.gameObject.SetActive(false);

  }

}



public void GestureProcess(SpacesHand hand)
{


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

          UpdateHandVisible(hand);
      }

      foreach (var hand in args.removed) {
          var gestureNameTextField = hand.IsLeft ? LeftHandGestureName : RightHandGestureName;
          var gestureRatioTextField = hand.IsLeft ? LeftHandGestureRatio : RightHandGestureRatio;
          var flipRatioTextField = hand.IsLeft ? LeftHandFlipRatio : RightHandFlipRatio;



          gestureNameTextField.text = "-";
          gestureRatioTextField.text = "-";
          flipRatioTextField.text = "-";

          HandExit(hand);
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
