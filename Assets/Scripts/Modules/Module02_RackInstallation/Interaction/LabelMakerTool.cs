using Framework.Interaction.Tools;
using Shared.Cabling;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Modules.Module02_RackInstallation.Interaction
{
    [RequireComponent(typeof(Tool))]
    public sealed class LabelMakerTool : MonoBehaviour
    {
        [SerializeField] private Transform nozzle;
        [SerializeField, Min(0.005f)] private float radius = 0.045f;
        [SerializeField] private InputActionReference triggerAction;
        [SerializeField] private TMP_Text feedback;
        private InputAction fallback;
        private InputAction Action => triggerAction != null ? triggerAction.action : fallback;
        private void Awake()
        {
            if (triggerAction == null)
                fallback = new InputAction("PrintCableLabel", InputActionType.Button, "<XRController>{RightHand}/triggerPressed");
        }
        private void OnEnable()
        {
            Action.performed += Print;
            Action.Enable();
        }
        private void OnDisable()
        {
            Action.performed -= Print;
            if (fallback != null) fallback.Disable();
        }
        private void OnDestroy() => fallback?.Dispose();
        private void Print(InputAction.CallbackContext _) => PrintNearest();
        [ContextMenu("Print nearest label")]
        public void PrintNearest()
        {
            if (!isActiveAndEnabled || nozzle == null || GetComponent<Tool>().Type != ToolType.LabelMaker) return;
            CableEndpointLabel nearest = null;
            float distance = float.PositiveInfinity;
            foreach (var hit in Physics.OverlapSphere(nozzle.position, radius, ~0, QueryTriggerInteraction.Collide))
            {
                var label = hit.GetComponentInParent<CableEndpointLabel>();
                if (label == null || !label.isActiveAndEnabled) continue;
                float candidate = (label.transform.position - nozzle.position).sqrMagnitude;
                if (candidate >= distance) continue;
                nearest = label; distance = candidate;
            }
            bool success = nearest != null && nearest.TryLabel();
            if (feedback != null) feedback.text = success ? "Etiqueta aplicada" : nearest == null
                ? "Acerca la boquilla\nal conector" : "Conecta ambos\nextremos primero";
        }
        private void OnDrawGizmosSelected()
        {
            if (nozzle == null) return;
            Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(nozzle.position, radius);
        }
    }
}
