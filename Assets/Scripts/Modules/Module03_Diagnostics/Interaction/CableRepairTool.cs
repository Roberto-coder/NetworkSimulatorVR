using System.Linq;
using Systems.Input;
using TMPro;
using UnityEngine;

namespace Modules.Module03_Diagnostics.Interaction
{
    /// <summary>Entrada de gatillo común; delega el tester XR o activa la etiqueta precolocada de un extremo.</summary>
    public sealed class CableRepairTool : MonoBehaviour
    {
        public enum Mode { Tester, LabelMaker }
        public Mode mode;
        public Transform tip;
        public TMP_Text feedback;
        private CableRepairController controller;
        private bool previousTrigger;
        // Compatibilidad con UnityEvents guardados en prefabs antiguos; ya no hay selección manual.
        public void SelectFirstLabel() { }
        public void SelectSecondLabel() { }
        private void Start()
        {
            controller = FindObjectsByType<CableRepairController>(FindObjectsSortMode.None)
                .SingleOrDefault(c => c.gameObject.scene == gameObject.scene);
            if (mode == Mode.LabelMaker)
                foreach (var button in GetComponentsInChildren<UnityEngine.UI.Button>(true))
                    if (button.name == "Etiqueta extremo A" || button.name == "Etiqueta extremo B")
                        button.gameObject.SetActive(false);
            ShowSelected();
        }
        private void ShowSelected()
        {
            if (feedback == null || controller == null) return;
            feedback.text = mode == Mode.Tester ? "Encaja ambos extremos en Master y Remote; pulsa gatillo" :
                "Acerca la punta a cada extremo del reemplazo y pulsa el gatillo para activar su etiqueta.";
        }
        private void Update()
        {
            bool pressed = VRInputManager.Instance != null && VRInputManager.Instance.RightTrigger >= 0.95f;
            if (pressed && !previousTrigger) Use();
            previousTrigger = pressed;
        }
        public void Use()
        {
            if (Time.timeScale <= 0 || controller == null || !controller.Ready || tip == null) return;
            var flow = Modules.Module03_Diagnostics.Flow.Module03GuidedFlow.Instance;
            if (flow != null && !flow.Allows(Modules.Module03_Diagnostics.Domain.DiagnosticStage.Cable)) return;
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
                    controller.Service.ActivateLabel(nearest.cable.cableId, nearest.a, out message);
            }
            if (feedback != null) feedback.text = message;
            controller.RefreshStatus();
        }
    }
}
