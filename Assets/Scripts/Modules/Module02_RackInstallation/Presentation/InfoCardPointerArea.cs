using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Modules.Module02_RackInstallation.Presentation
{
    public sealed class InfoCardPointerArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private readonly HashSet<int> pointers = new();
        public bool IsHovered => pointers.Count > 0;
        public void OnPointerEnter(PointerEventData data) => pointers.Add(data.pointerId);
        public void OnPointerExit(PointerEventData data) => pointers.Remove(data.pointerId);
        private void OnDisable() => pointers.Clear();
    }
}
