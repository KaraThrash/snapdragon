using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour
{

  public TextHoverPanel primary;
  public TextHoverPanel secondary;
  public TextHoverPanel other;

    public void Update()
    {



    }

    public void ShowPanels(bool _on)
    {
      if(primary != null )
      {
          primary.gameObject.SetActive(_on);

      }
      if(secondary != null )
      {
          secondary.gameObject.SetActive(_on);

      }
      if(other != null )
      {
          other.gameObject.SetActive(_on);

      }
    }

    public void SetPanel(TextHoverPanel _panel)
    {
      if(primary == null)
      {
        primary = _panel;

      }else if(secondary == null){secondary = _panel;}
      else{other = _panel;}
    }


}
