// Solo dibuja.
//
//     Nunca sabe:
//
// qué cable es
//     si está correcto
// si está conectado
//
//     Solo recibe dos puntos.

using UnityEngine;
using Modules.Module01_CableMaking.Domain;
using Modules.Module01_CableMaking.Domain.Cable;

namespace Modules.Module01_CableMaking.Presentation
{
    [RequireComponent(typeof(LineRenderer))]
    public class WireRenderer : MonoBehaviour
    {
        [SerializeField]
        private Wire wire;

        private LineRenderer line;

        void Awake()
        {
            line = GetComponent<LineRenderer>();

            line.positionCount = 2;
            line.useWorldSpace = true;
        }

        void LateUpdate()
        {
            line.SetPosition(0, wire.StartPoint.position);
            line.SetPosition(1, wire.transform.position);
        }
    }
}