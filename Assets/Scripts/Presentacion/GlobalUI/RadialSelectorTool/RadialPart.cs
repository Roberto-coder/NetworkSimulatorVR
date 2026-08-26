using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Presentacion.GlobalUI.RadialSelectorTool
{
    /// <summary>A raycastable wedge and its hover visuals.</summary>
    public sealed class RadialPart : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ICanvasRaycastFilter
    {
        private RadialMenuSelector selector;
        private Image background;
        private float visibleAngle;
        private float innerRadiusFactor;

        public int Index { get; private set; }

        public void Initialize(int index, RadialMenuSelector owner, Image image,
            float wedgeAngle, float innerRadius)
        {
            Index = index;
            selector = owner;
            background = image;
            visibleAngle = wedgeAngle;
            innerRadiusFactor = innerRadius;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            selector.Select(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (selector.SelectedIndex == Index)
                selector.Select(null);
        }

        public void SetHighlighted(bool highlighted)
        {
            if (background != null)
                background.color = highlighted ? Color.green : Color.white;
            transform.localScale = highlighted ? Vector3.one * 1.08f : Vector3.one;
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            RectTransform rect = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, eventCamera, out Vector2 local))
                return false;

            float radius = Mathf.Min(rect.rect.width, rect.rect.height) * 0.5f;
            float distance = local.magnitude;
            if (distance < radius * innerRadiusFactor || distance > radius)
                return false;

            float clockwiseAngle = -Vector2.SignedAngle(Vector2.up, local);
            if (clockwiseAngle < 0f)
                clockwiseAngle += 360f;
            return clockwiseAngle <= visibleAngle;
        }
    }
}
