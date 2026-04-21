using System.Collections.Generic;
using UnityEngine;

public class SetColorFromList : MonoBehaviour
{
     List<Color> colors = new List<Color>();

     public void SetColor(int i)
     {
          GetComponent<Renderer>().material.color = colors[i];
     }
}
