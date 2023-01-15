using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour
{

  public TextHoverPanel primary;
  public TextHoverPanel secondary;
  public TextHoverPanel otherPanel;

    public void Update()
    {
        


    }

    public void SetPanels(TextHoverPanel _a,TextHoverPanel _b,TextHoverPanel _c)
    {
        primary = _a;secondary = _b;
        otherPanel = _c;

    }




}
