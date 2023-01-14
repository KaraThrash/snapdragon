using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class TextHoverPanel : MonoBehaviour
{

  public bool hover;
  public float speed;
  public float rotSpeed;
  public List<Text> textRows;
  public Transform observer;
  public Transform anchor;


  public Sprite red_titleBackgroundImage;
  public Sprite blue_titleBackgroundImage;


  public Image headerBackground;
  public Image backgroundPanel;

  public Color blueHeader;
  public Color redHeader;

  public Vector3 anchorOffset;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(hover)
        {
            Hover();

        }

        if(Keyboard.current.spaceKey.isPressed )
        {
          SetColor(1);
          Debug.Log("Pressed");

            }else
          // if(Input.GetKeyUp("Space"))
          {
            Debug.Log("Dont!");
            // SetColor(0);

          }
    }


    public void Hover()
    {
      if(observer == null ){return;}



      Vector3 hoverPosition = anchor.position + anchorOffset;
      float dist = Vector3.Distance(hoverPosition,transform.position);

      transform.position = Vector3.MoveTowards(transform.position, hoverPosition, Time.deltaTime * speed * dist);

      Quaternion newRot = Quaternion.LookRotation(transform.position - observer.position);
      transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * rotSpeed * (1 + (dist * 0.1f)) );



    }








    public void SetColor(int _format)
    {

      Color clr = redHeader;
      Sprite headerImg = red_titleBackgroundImage;
      //red format
      if(_format == 0)
      {
            clr = redHeader;
             headerImg = red_titleBackgroundImage;

      }
      else if(_format == 1)
      {
          clr = blueHeader;
           headerImg = blue_titleBackgroundImage;

      }

        if(backgroundPanel)
        {
          backgroundPanel.color = (clr);
        }
        if(headerBackground)
        {
          headerBackground.sprite = headerImg;
        }

    }


    public void SetRowInformation(int _row,string _text)
    {
        if(textRows != null)
        {
          if(textRows.Count > _row)
          {
            textRows[_row].text = _text;

          }

        }

    }










}
