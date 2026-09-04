using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.Module02_RackInstallation.Exploration
{
    /// <summary>Arbitra ambos punteros sin cancelar el dwell al salir sólo uno.</summary>
    public sealed class InfoFocusDetector : MonoBehaviour
    {
        public static InfoFocusDetector Instance { get; private set; }
        [Min(0.1f)] [SerializeField] private float dwellDuration = 1.5f;
        private readonly Dictionary<object, RackInfoTarget> sources = new();
        private RackInfoTarget currentTarget;
        private float focusStartedAt;
        private bool requested;
        public RackInfoTarget CurrentTarget => currentTarget;
        public event Action<RackInfoTarget, float> FocusProgressChanged;
        public event Action<RackInfoTarget> CardRequested;
        public event Action<RackInfoTarget> FocusCleared;

        private void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Sólo puede haber un InfoFocusDetector activo.", this);
                enabled = false;
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (currentTarget == null || !currentTarget.isActiveAndEnabled)
            {
                if (sources.Count > 0) ResetFocus();
                return;
            }
            if (requested) return;
            float progress = Mathf.Clamp01((Time.unscaledTime - focusStartedAt) / Mathf.Max(0.1f, dwellDuration));
            FocusProgressChanged?.Invoke(currentTarget, progress);
            if (progress < 1f) return;
            requested = true;
            CardRequested?.Invoke(currentTarget);
        }

        public void BeginFocus(RackInfoTarget target) => BeginFocus(target, target);
        public void BeginFocus(RackInfoTarget target, object source)
        {
            if (target == null || source == null || target.Information == null) return;
            sources[source] = target;
            SetCurrent(target.ResolvedTarget);
        }

        public void EndFocus(RackInfoTarget target, object source)
        {
            // Un exit tardío de A no debe borrar un hover nuevo de B del mismo ray.
            if (source == null || !sources.TryGetValue(source, out var registered) || registered != target) return;
            sources.Remove(source);
            Reconcile();
        }

        public void ClearFocus(RackInfoTarget target)
        {
            var keys = new List<object>();
            foreach (var pair in sources)
                if (pair.Value == target || pair.Value == null || pair.Value.ResolvedTarget == target) keys.Add(pair.Key);
            foreach (object key in keys) sources.Remove(key);
            Reconcile();
        }

        public void SuppressCurrent() => requested = true;
        public void RestartCurrent()
        {
            requested = false;
            focusStartedAt = Time.unscaledTime;
        }

        private void Reconcile()
        {
            RackInfoTarget fallback = null;
            foreach (var pair in sources)
            {
                if (pair.Value == null || !pair.Value.isActiveAndEnabled) continue;
                var resolved = pair.Value.ResolvedTarget;
                if (resolved == currentTarget) return;
                fallback = resolved;
            }
            SetCurrent(fallback);
        }

        private void SetCurrent(RackInfoTarget target)
        {
            if (currentTarget == target) return;
            RackInfoTarget old = currentTarget;
            currentTarget = target;
            RestartCurrent();
            if (old != null) FocusCleared?.Invoke(old);
            if (target != null) FocusProgressChanged?.Invoke(target, 0f);
        }

        private void ResetFocus()
        {
            sources.Clear();
            SetCurrent(null);
        }

        private void OnDisable()
        {
            ResetFocus();
            if (Instance == this) Instance = null;
        }
    }
}
