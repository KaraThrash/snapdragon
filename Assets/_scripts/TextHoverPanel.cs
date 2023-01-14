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

  public List<Sprite> header_backgroundImages;
  public List<Color> backgroundColors;
  public List<Sprite> arrowImages;


  public Sprite red_titleBackgroundImage;
  public Sprite blue_titleBackgroundImage;


  public Image upArrowImage;
  public Image downArrowImage;
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

      Color clr = Color.red;
      Sprite headerImg= null;
      Sprite upArrow= null ;
      Sprite downArrow = null;
      //red format
      if(header_backgroundImages != null & header_backgroundImages.Count > _format)
      {
            headerImg = header_backgroundImages[_format];

      }
      if(backgroundColors != null & backgroundColors.Count > _format)
      {
            clr = backgroundColors[_format];

      }
      if(arrowImages != null & arrowImages.Count > _format * 2)
      {
            upArrow = arrowImages[_format];
            downArrow = arrowImages[_format + 1];

      }


      else if(_format == 1)
      {
          clr = blueHeader;
           headerImg = blue_titleBackgroundImage;

      }

        if(backgroundPanel)
        {
          backgroundPanel.color = clr;
        }
        if(headerBackground)
        {
          headerBackground.sprite = headerImg;
        }
        if(upArrowImage)
        {
          upArrowImage.sprite = upArrow;
        }
        if(downArrowImage)
        {
          downArrowImage.sprite = downArrow;
        }

    }

private int currentStyle;

public void NextStyle()
{
  currentStyle++;
  if(currentStyle >= backgroundColors.Count)
  {currentStyle = 0;}
  SetColor(currentStyle);

}

public void NextStyle(int _format)
{
  currentStyle = _format % backgroundColors.Count;
  if(currentStyle >= backgroundColors.Count)
  {currentStyle = 0;}
  SetColor(currentStyle);

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
