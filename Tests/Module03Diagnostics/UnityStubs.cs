// Permite probar los adaptadores de datos sin abrir Unity; no sustituye una compilación del Editor.
namespace UnityEngine
{
    public class ScriptableObject { }
    public sealed class SerializeField : System.Attribute { }
    public sealed class CreateAssetMenuAttribute : System.Attribute
    {
        public string menuName;
        public string fileName;
    }
}
