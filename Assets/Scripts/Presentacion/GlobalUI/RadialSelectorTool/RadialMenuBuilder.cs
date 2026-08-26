using System.Collections.Generic;
using Framework.Interaction.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Presentacion.GlobalUI.RadialSelectorTool
{
    /// <summary>Creates and lays out the visual parts of the radial menu.</summary>
    public sealed class RadialMenuBuilder : MonoBehaviour
    {
        private readonly List<GameObject> spawnedParts = new List<GameObject>();

        public void Build(GameObject prefab, Transform parent, IReadOnlyList<ToolData> tools,
            float gapDegrees, float iconRadiusFactor, Vector2 iconSize, float innerRadiusFactor,
            RadialMenuSelector selector)
        {
            Clear();
            int count = tools.Count;
            float step = 360f / count;
            float gap = Mathf.Clamp(gapDegrees, 0f, step * 0.9f);
            float visibleAngle = step - gap;

            for (int i = 0; i < count; i++)
            {
                GameObject instance = Instantiate(prefab, parent);
                RectTransform rect = instance.GetComponent<RectTransform>();
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;

                // A radial Image starts at 12 o'clock and fills clockwise. Rotate its
                // start edge so the visible wedge is centred on this tool's index.
                float centreClockwise = i * step;
                float startClockwise = centreClockwise - visibleAngle * 0.5f;
                rect.localEulerAngles = new Vector3(0f, 0f, -startClockwise);

                Image background = instance.GetComponent<Image>();
                background.type = Image.Type.Filled;
                background.fillMethod = Image.FillMethod.Radial360;
                background.fillOrigin = (int)Image.Origin360.Top;
                background.fillClockwise = true;
                background.fillAmount = visibleAngle / 360f;
                background.raycastTarget = true;

                ConfigureIcon(instance.transform, tools[i], visibleAngle, centreClockwise,
                    iconRadiusFactor, iconSize);

                RadialPart part = instance.GetComponent<RadialPart>();
                if (part == null)
                    part = instance.AddComponent<RadialPart>();
                part.Initialize(i, selector, background, visibleAngle, innerRadiusFactor);
                selector.Register(part);
                spawnedParts.Add(instance);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < spawnedParts.Count; i++)
                if (spawnedParts[i] != null)
                    Destroy(spawnedParts[i]);
            spawnedParts.Clear();
        }

        private static void ConfigureIcon(Transform part, ToolData tool, float visibleAngle,
            float centreClockwise, float radiusFactor, Vector2 size)
        {
            Transform icon = part.Find("Icon");
            if (icon == null)
                return;

            RectTransform partRect = (RectTransform)part;
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = iconRect.anchorMax = iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = size;
            iconRect.localScale = Vector3.one;

            float radius = Mathf.Min(partRect.rect.width, partRect.rect.height) * 0.5f * radiusFactor;
            Vector3 localDirection = Quaternion.Euler(0f, 0f, -visibleAngle * 0.5f) * Vector3.up;
            iconRect.anchoredPosition = new Vector2(localDirection.x, localDirection.y) * radius;
            iconRect.localEulerAngles = new Vector3(0f, 0f, centreClockwise);

            Image iconImage = icon.GetComponent<Image>() ?? icon.GetComponentInChildren<Image>(true);
            if (iconImage == null)
                return;
            iconImage.sprite = tool != null ? tool.icon : null;
            iconImage.enabled = iconImage.sprite != null;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
        }
    }
}
