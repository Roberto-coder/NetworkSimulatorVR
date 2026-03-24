using UnityEngine;

[System.Serializable]
public class ModuloData
{
    public string titulo;
    public string descripcion;
    [TextArea(3, 10)]
    public string objetivos;
    public Sprite imagen;
    public string escena;
}