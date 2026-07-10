using System.Collections.Generic;
using UnityEngine;

namespace Presentacion.GlobalUI.RadialSelectorTool
{
     public class SetColorFromList : MonoBehaviour
     {
          List<Color> colors = new List<Color>();

          public void SetColor(int i)
          {
               GetComponent<Renderer>().material.color = colors[i];
          }
     }
}
