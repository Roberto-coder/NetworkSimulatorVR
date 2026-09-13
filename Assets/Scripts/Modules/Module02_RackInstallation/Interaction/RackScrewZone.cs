using Modules.Module02_RackInstallation.Flow.Validation;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Modules.Module02_RackInstallation.Interaction
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class RackScrewZone : MonoBehaviour
    {
        [SerializeField] private RackFasteningAssembly assembly;
        [Min(0.1f)] [SerializeField] private float holdSeconds = 2f;
        [SerializeField] private Renderer screwVisual;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private AudioSource feedbackAudio;
        private readonly HashSet<ScrewdriverTip> tips = new();
        private MaterialPropertyBlock block;
        private float elapsed;
        public bool IsFastened { get; private set; }
        public float Progress => Mathf.Clamp01(elapsed / holdSeconds);

        private void Awake()
        {
            GetComponent<SphereCollider>().isTrigger = true;
            Refresh();
        }
        private void OnDisable() { tips.Clear(); if (!IsFastened) elapsed = 0; }
        private void OnTriggerEnter(Collider other)
        {
            var tip = other.GetComponent<ScrewdriverTip>();
            if (tip != null) tips.Add(tip);
        }
        private void OnTriggerExit(Collider other)
        {
            var tip = other.GetComponent<ScrewdriverTip>();
            if (tip != null) tips.Remove(tip);
        }
        private void Update()
        {
            if (IsFastened) return;
            // Destroying/unequipping a tool need not emit OnTriggerExit.
            tips.RemoveWhere(t => t == null || !t.IsUsable);
            bool working = assembly != null && assembly.CanFasten && tips.Count > 0 &&
                Flow.Module02SequenceCoordinator.Allow(Module02Action.Fasten);
            elapsed = working ? Mathf.Min(holdSeconds, elapsed + Time.deltaTime) : 0;
            if (elapsed >= holdSeconds)
            {
                IsFastened = true;
                if (feedbackAudio != null && feedbackAudio.clip != null) feedbackAudio.Play();
                assembly.NotifyScrewFastened();
            }
            Refresh();
        }
        public void ResetFastening()
        {
            elapsed = 0; IsFastened = false; tips.Clear(); Refresh();
        }
        private void Refresh()
        {
            if (statusLabel != null) statusLabel.text = IsFastened ? "Fijado" :
                elapsed > 0 ? $"{Progress:P0}" : $"{holdSeconds:0.#} s";
            if (screwVisual == null) return;
            block ??= new MaterialPropertyBlock();
            screwVisual.GetPropertyBlock(block);
            Color color = IsFastened ? Color.green : Color.Lerp(Color.gray, Color.yellow, Progress);
            block.SetColor("_BaseColor", color); block.SetColor("_Color", color);
            screwVisual.SetPropertyBlock(block);
        }
    }
}
