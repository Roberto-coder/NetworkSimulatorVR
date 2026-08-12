using Oculus.Interaction;
using UnityEngine;

namespace Modules.Module01_CableMaking.Interaction
{
    /// <summary>
    /// Evita que Grabbable aplique su movimiento 3D libre por defecto.
    /// WireGrabHandler mueve la punta dentro del plano del puzzle.
    /// </summary>
    public sealed class WireGrabTransformer : MonoBehaviour, ITransformer
    {
        public void Initialize(IGrabbable grabbable) { }
        public void BeginTransform() { }
        public void UpdateTransform() { }
        public void EndTransform() { }
    }
}
