// Permite probar los adaptadores de datos sin abrir Unity; no sustituye una compilaciÃ³n del Editor.
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

namespace UnityEngine { public static class Debug { public static void Log(object message) { } } }
// Solo el contrato de datos; el flujo probado es el de producción.
namespace GameData.Modules
{
    public class ModuleDefinition
    {
        public System.Collections.Generic.List<GameData.Objectives.ObjectiveData> Objectives { get; } = new();
    }
}
