using System.Linq;
using Systems.Input;
using TMPro;
using UnityEngine;

namespace Modules.Module03_Diagnostics.Interaction
{
    /// <summary>Entrada de gatillo común; delega el tester XR o aplica la etiqueta elegida a un extremo.</summary>
    public sealed class CableRepairTool : MonoBehaviour
    {
        public enum Mode { Tester, LabelMaker }
        public Mode mode;
        public Transform tip;
        public TMP_Text feedback;
        private CableRepairController controller;
        private bool previousTrigger;
        private bool secondLabel;
        public void SelectFirstLabel() { secondLabel = false; ShowSelected(); }
        public void SelectSecondLabel() { secondLabel = true; ShowSelected(); }
        private void Start()
        {
            controller = FindObjectsByType<CableRepairController>(FindObjectsSortMode.None)
                .SingleOrDefault(c => c.gameObject.scene == gameObject.scene);
            ShowSelected();
        }
        private void ShowSelected()
        {
            if (feedback == null || controller == null) return;
            feedback.text = mode == Mode.Tester ? "Encaja ambos extremos en Master y Remote; pulsa gatillo" :
                "Imprimir: " + (secondLabel ? controller.settings.incident.labelAtB : controller.settings.incident.labelAtA);
        }
        private void Update()
        {
            bool pressed = VRInputManager.Instance != null && VRInputManager.Instance.RightTrigger >= 0.95f;
            if (pressed && !previousTrigger) Use();
            previousTrigger = pressed;
        }
        public void Use()
        {
            if (controller == null || !controller.Ready || tip == null) return;
            controller.SyncConnections();
            string message = "Acerca la herramienta al cable.";
            if (mode == Mode.Tester)
            {
                var tester = GetComponent<XRCableTester>();
                if (tester != null) { tester.BeginTest(); return; }
                message = "Actualizar este prefab a la versión Tester XR desde el configurador.";
            }
            else
            {
                var nearest = controller.cables.Where(c => c.isActiveAndEnabled)
                    .SelectMany(c => new[] { (cable: c, a: true, pos: c.endA.transform.position), (cable: c, a: false, pos: c.endB.transform.position) })
                    .OrderBy(e => Vector3.Distance(tip.position, e.pos)).FirstOrDefault();
                if (nearest.cable != null && Vector3.Distance(tip.position, nearest.pos) <= controller.settings.labelRadius)
                    controller.Service.LabelEnd(nearest.cable.cableId, nearest.a,
                        secondLabel ? controller.settings.incident.labelAtB : controller.settings.incident.labelAtA, out message);
            }
            if (feedback != null) feedback.text = message;
            controller.RefreshStatus();
        }
    }
}
