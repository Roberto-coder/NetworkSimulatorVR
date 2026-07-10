using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Presentacion.GlobalUI.RadialSelectorTool
{
    public class RadialSelectionComponent : MonoBehaviour
    {
        [Header("UI")]
        public GameObject radialPartPrefab;
        public Transform radialPartCanvas;
        public float distanceFromCamera = 1.5f;
        public float verticalOffset = 0f;

        [Header("Radial Settings")]
        [Min(0f)]
        public float angleBetweenParts = 10f;
        [Range(0f, 1f)]
        public float iconRadiusFactor = 0.65f;
        public Vector2 iconSize = new Vector2(80f, 80f);

        [Header("Input")]
        public OVRInput.Button spawnButton;
        public Transform rightHandTransform;
        public Camera playerCamera;

        [Header("Tools")]
        public List<ToolData> tools;
        public ToolManager toolManager;

        [Header("Debug")]
        public bool debugLogs = true;
        [Min(0)]
        public int inputHeartbeatFrames = 120;

        private List<GameObject> spawnedParts = new List<GameObject>();
        private int currentIndex = -1;
        private int lastLoggedIndex = -1;
        private string lastSelectionSkipReason;
        private bool menuActive = false;
        private int RadialPartCount => tools == null ? 0 : tools.Count;

        void Start()
        {
            LogDebug(
                $"Start. Button={spawnButton}, Tools={RadialPartCount}, " +
                $"Prefab={(radialPartPrefab == null ? "NULL" : radialPartPrefab.name)}, " +
                $"Canvas={(radialPartCanvas == null ? "NULL" : radialPartCanvas.name)}, " +
                $"Camera={(playerCamera == null ? "NULL" : playerCamera.name)}, " +
                $"RightHand={(rightHandTransform == null ? "NULL" : rightHandTransform.name)}"
            );

            if (radialPartCanvas != null)
                radialPartCanvas.gameObject.SetActive(false);
        }

        void Update()
        {
            if (debugLogs && inputHeartbeatFrames > 0 && Time.frameCount % inputHeartbeatFrames == 0)
            {
                LogDebug($"Polling input. Button={spawnButton}, Held={OVRInput.Get(spawnButton)}, MenuActive={menuActive}");
            }

            if (OVRInput.GetDown(spawnButton))
            {
                LogDebug($"GetDown detected for {spawnButton}");
                ShowMenu();
            }

            if (OVRInput.Get(spawnButton) && menuActive)
                UpdateSelection();

            if (OVRInput.GetUp(spawnButton) && menuActive)
            {
                LogDebug($"GetUp detected for {spawnButton}");
                ConfirmSelection();
            }
        }

        void ShowMenu()
        {
            LogDebug("ShowMenu requested.");

            if (RadialPartCount == 0)
            {
                LogDebug("ShowMenu cancelled: tools list is empty.");
                return;
            }

            if (radialPartPrefab == null)
            {
                LogDebug("ShowMenu cancelled: radialPartPrefab is not assigned.");
                return;
            }

            if (radialPartCanvas == null)
            {
                LogDebug("ShowMenu cancelled: radialPartCanvas is not assigned.");
                return;
            }

            if (playerCamera == null)
            {
                LogDebug("ShowMenu cancelled: playerCamera is not assigned.");
                return;
            }

            menuActive = true;
            currentIndex = -1;
            lastLoggedIndex = -1;

            Transform cam = playerCamera.transform;

            // Posición frente al usuario
            Vector3 radialForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
            Vector3 radialPosition = cam.position + radialForward.normalized * distanceFromCamera;
            radialPosition.y = cam.position.y + verticalOffset;
            radialPartCanvas.position = radialPosition;

            // Rotación estable
            Vector3 forward = radialForward;
            forward.y = 0;

            if (forward.sqrMagnitude > 0.001f)
                radialPartCanvas.rotation = Quaternion.LookRotation(forward);

            radialPartCanvas.gameObject.SetActive(true);
            LogDebug($"Canvas activated at {radialPartCanvas.position}. Spawning {RadialPartCount} radial parts.");

            SpawnParts();
        }

        void ConfirmSelection()
        {
            LogDebug($"ConfirmSelection. CurrentIndex={currentIndex}, Tools={RadialPartCount}, ToolManager={(toolManager == null ? "NULL" : toolManager.name)}");

            if (currentIndex >= 0 && currentIndex < RadialPartCount && toolManager != null)
            {
                toolManager.EquipTool(tools[currentIndex]);
                LogDebug($"Equipped tool index {currentIndex}: {(tools[currentIndex] == null ? "NULL" : tools[currentIndex].name)}");
            }

            if (radialPartCanvas != null)
                radialPartCanvas.gameObject.SetActive(false);

            menuActive = false;
        }

        void UpdateSelection()
        {
            if (spawnedParts.Count == 0)
            {
                LogSelectionSkip("no spawned parts.");
                return;
            }

            if (rightHandTransform == null)
            {
                LogSelectionSkip("rightHandTransform is not assigned.");
                return;
            }

            if (radialPartCanvas == null)
            {
                LogSelectionSkip("radialPartCanvas is not assigned.");
                return;
            }

            Vector3 dir = rightHandTransform.position - radialPartCanvas.position;
            Vector3 projected = Vector3.ProjectOnPlane(dir, radialPartCanvas.forward);
            if (projected.sqrMagnitude <= 0.0001f)
            {
                LogSelectionSkip("hand direction is too close to the radial center.");
                return;
            }

            lastSelectionSkipReason = null;

            float angle = Vector3.SignedAngle(radialPartCanvas.up, projected, -radialPartCanvas.forward);
            if (angle < 0) angle += 360f;

            currentIndex = Mathf.Clamp(
                (int)(angle * spawnedParts.Count / 360f),
                0,
                spawnedParts.Count - 1
            );

            if (currentIndex != lastLoggedIndex)
            {
                lastLoggedIndex = currentIndex;
                LogDebug($"Selection changed. Index={currentIndex}, Angle={angle:0.0}");
            }

            Highlight();
        }

        void Highlight()
        {
            for (int i = 0; i < spawnedParts.Count; i++)
            {
                if (spawnedParts[i] == null) continue;

                Image img = spawnedParts[i].GetComponent<Image>();

                if (i == currentIndex)
                {
                    if (img != null) img.color = Color.green;
                    spawnedParts[i].transform.localScale = Vector3.one * 1.1f;
                }
                else
                {
                    if (img != null) img.color = Color.white;
                    spawnedParts[i].transform.localScale = Vector3.one;
                }
            }
        }

        void SpawnParts()
        {
            LogDebug($"SpawnParts started. ExistingParts={spawnedParts.Count}");

            foreach (var part in spawnedParts)
                if (part != null) Destroy(part);

            spawnedParts.Clear();

            int partCount = RadialPartCount;
            if (partCount == 0)
            {
                LogDebug("SpawnParts cancelled: partCount is 0.");
                return;
            }

            float step = 360f / partCount;
            float gap = Mathf.Clamp(angleBetweenParts, 0f, step * 0.9f);
            float halfStep = step / 2f;
            float halfGap = gap / 2f;

            for (int i = 0; i < partCount; i++)
            {
                float centerAngle = -i * step;                       // centro lógico del sector i
                float fillAngle = centerAngle - halfStep - halfGap;   // ángulo usado solo para alinear el fillAmount

                GameObject part = Instantiate(radialPartPrefab, radialPartCanvas);
                part.transform.localPosition = Vector3.zero;
                part.transform.localEulerAngles = new Vector3(0, 0, fillAngle);

                Image img = part.GetComponent<Image>();
                if (img != null)
                {
                    img.type = Image.Type.Filled;
                    img.fillMethod = Image.FillMethod.Radial360;
                    img.fillAmount = (step - gap) / 360f;
                }

                if (tools[i] != null)
                    SetupIcon(part.transform, tools[i], fillAngle, centerAngle);

                spawnedParts.Add(part);
            }
        }

        void SetupIcon(Transform part, ToolData tool, float fillAngle, float centerAngle)
        {
            Transform iconRoot = part.Find("Icon");
            if (iconRoot == null) return;

            RectTransform iconRect = iconRoot.GetComponent<RectTransform>();
            if (iconRect != null)
            {
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.sizeDelta = iconSize;

                // Cuánto hay que "corregir" respecto al ángulo con el que gira 'part'
                float offsetAngle = centerAngle - fillAngle;
                iconRect.anchoredPosition = PolarOffset(offsetAngle, GetIconRadius(part));

                // Mantiene el sprite del ícono sin inclinarse aunque 'part' esté rotado
                iconRect.localEulerAngles = new Vector3(0f, 0f, -fillAngle);
            }

            Image iconImage = iconRoot.GetComponent<Image>() ?? iconRoot.GetComponentInChildren<Image>(true);
            if (iconImage == null) return;

            iconImage.enabled = tool.icon != null;
            iconImage.sprite = tool.icon;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            RectTransform imageRect = iconImage.GetComponent<RectTransform>();
            if (imageRect != null && imageRect != iconRoot.GetComponent<RectTransform>())
            {
                imageRect.anchorMin = Vector2.zero;
                imageRect.anchorMax = Vector2.one;
                imageRect.offsetMin = Vector2.zero;
                imageRect.offsetMax = Vector2.zero;
                imageRect.localEulerAngles = Vector3.zero;
                imageRect.localScale = Vector3.one;
            }
        }

        Vector2 PolarOffset(float angleDeg, float radius)
        {
            Vector3 dir = Quaternion.Euler(0f, 0f, angleDeg) * Vector3.up;
            return new Vector2(dir.x, dir.y) * radius;
        }

        float GetIconRadius(Transform part)
        {
            RectTransform partRect = part.GetComponent<RectTransform>();

            if (partRect == null)
                return 0f;

            float radius = Mathf.Min(partRect.rect.width, partRect.rect.height) * 0.5f;
            return radius * iconRadiusFactor;
        }

        void LogDebug(string message)
        {
            if (!debugLogs)
                return;

            Debug.Log($"[RadialSelectionComponent] {message}", this);
        }

        void LogSelectionSkip(string reason)
        {
            if (lastSelectionSkipReason == reason)
                return;

            lastSelectionSkipReason = reason;
            LogDebug($"UpdateSelection skipped: {reason}");
        }
    }
}
