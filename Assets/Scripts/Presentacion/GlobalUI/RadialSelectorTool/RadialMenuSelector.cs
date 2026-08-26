using System.Collections.Generic;
using UnityEngine;

namespace Presentacion.GlobalUI.RadialSelectorTool
{
    /// <summary>Owns hover selection received from the Meta RayCanvas/EventSystem.</summary>
    public sealed class RadialMenuSelector : MonoBehaviour
    {
        private readonly List<RadialPart> parts = new List<RadialPart>();

        public int SelectedIndex { get; private set; } = -1;

        public void Register(RadialPart part)
        {
            parts.Add(part);
            part.SetHighlighted(false);
        }

        public void Select(RadialPart selected)
        {
            SelectedIndex = selected != null ? selected.Index : -1;
            for (int i = 0; i < parts.Count; i++)
                if (parts[i] != null)
                    parts[i].SetHighlighted(parts[i] == selected);
        }

        public void ClearSelection()
        {
            Select(null);
            parts.RemoveAll(part => part == null);
        }
    }
}
