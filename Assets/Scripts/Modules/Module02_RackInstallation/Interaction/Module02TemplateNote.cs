using UnityEngine;

namespace Modules.Module02_RackInstallation.Interaction
{
    /// <summary>Nota de autoría visible en el Inspector; no ejecuta lógica durante el juego.</summary>
    public sealed class Module02TemplateNote : MonoBehaviour
    {
        [TextArea(2, 5)] [SerializeField] private string instructions;
        public string Instructions => instructions;

#if UNITY_EDITOR
        public void Set(string value) => instructions = value;
#endif
    }
}
