using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Module02_RackInstallation.Presentation
{
    /// <summary>
    /// Dibuja el estado de la práctica y conecta el botón de confirmación.
    /// Recibe valores del flujo; nunca decide si un objetivo está completado.
    /// </summary>
    public sealed class Module02SequencePresenter : IDisposable
    {
        private readonly Button confirm;
        private readonly TMP_Text status;
        private readonly UnityEngine.Events.UnityAction onConfirm;
        private string rejection;
        private float rejectionUntil;

        public Module02SequencePresenter(Button confirm, TMP_Text status, Action onConfirm)
        {
            this.confirm = confirm;
            this.status = status;
            this.onConfirm = () => onConfirm();
            if (confirm != null) confirm.onClick.AddListener(this.onConfirm);
        }

        public void Show(int step, int total, string title, bool canConfirm,
            int reviewed, int required, int fastened, int connected, int labeled)
        {
            if (confirm != null) confirm.interactable = canConfirm;
            if (status == null) return;
            string progress = step <= 1 ? $"Fichas revisadas: {reviewed}/{required}" :
                step == 3 ? $"Pestañas: {fastened}/2" : step >= 4 && step <= 7 ?
                $"Conexiones: {connected}/5 · Etiquetas: {labeled}/10" : "";
            status.text = Time.unscaledTime < rejectionUntil ? rejection : step >= total ?
                "Práctica completada" : $"{step + 1}/{total} · {title}\n{progress}";
        }

        public void ShowRejection(string message)
        {
            rejection = message;
            rejectionUntil = Time.unscaledTime + 4f;
        }

        public void Dispose()
        {
            if (confirm != null) confirm.onClick.RemoveListener(onConfirm);
        }
    }
}
