using Framework.Interaction.Tools;
using Systems.Input;
using UnityEngine;

namespace Presentacion.GlobalUI.RadialSelectorTool
{
    /// <summary>Coordinates menu lifetime and equips the part selected by the Meta ray.</summary>
    public class RadialMenuController : MonoBehaviour
    {
        [Header("UI")]
        public GameObject radialPartPrefab;
        public Transform radialPartCanvas;
        public float distanceFromCamera = 1.5f;
        public float verticalOffset;

        [Header("Radial Settings")]
        [Min(0f)] public float angleBetweenParts = 10f;
        [Range(0f, 1f)] public float iconRadiusFactor = 0.65f;
        public Vector2 iconSize = new Vector2(110f, 110f);
        [Range(0f, 0.9f)] public float innerRadiusFactor = 0.38f;

        [Header("Input")]
        [Tooltip("Kept only so existing prefab/scene references are not lost. Selection now comes from RayCanvas.")]
        public Transform rightHandTransform;
        public Camera playerCamera;

        [Header("Tools")]
        public ToolManager toolManager;

        [Header("Debug")]
        public bool debugLogs;
        [Min(0)] public int inputHeartbeatFrames = 120;

        private RadialMenuBuilder builder;
        private RadialMenuSelector selector;
        private bool menuActive;

        protected virtual void Awake()
        {
            builder = GetOrAdd<RadialMenuBuilder>();
            selector = GetOrAdd<RadialMenuSelector>();
        }

        protected virtual void Start()
        {
            ResolveRuntimeReferences();

            if (radialPartCanvas != null)
                radialPartCanvas.gameObject.SetActive(false);
        }

        protected virtual void Update()
        {
            if (VRInputManager.Instance == null)
                return;

            if (VRInputManager.Instance.ToolPressed)
                ShowMenu();

            if (menuActive && VRInputManager.Instance.ToolReleased)
                ConfirmSelection();
        }

        public void ShowMenu()
        {
            if (!CanOpen())
                return;

            PositionInFrontOfPlayer();
            radialPartCanvas.gameObject.SetActive(true);
            selector.ClearSelection();
            builder.Build(radialPartPrefab, radialPartCanvas, toolManager.AvailableTools,
                angleBetweenParts, iconRadiusFactor, iconSize, innerRadiusFactor, selector);
            menuActive = true;
        }

        public void ConfirmSelection()
        {
            int index = selector.SelectedIndex;
            if (index >= 0 && index < toolManager.AvailableTools.Count)
                toolManager.EquipTool(toolManager.AvailableTools[index]);

            CloseMenu();
        }

        public void CloseMenu()
        {
            menuActive = false;
            selector.ClearSelection();
            if (radialPartCanvas != null)
                radialPartCanvas.gameObject.SetActive(false);
        }

        private bool CanOpen()
        {
            bool valid = radialPartPrefab != null && radialPartCanvas != null &&
                         playerCamera != null && toolManager != null &&
                         toolManager.AvailableTools != null && toolManager.AvailableTools.Count > 0;
            if (!valid && debugLogs)
                Debug.LogWarning("[RadialMenu] Missing prefab, canvas, camera, ToolManager, or available tools.", this);
            return valid;
        }

        private void ResolveRuntimeReferences()
        {
            if (toolManager == null)
                toolManager = FindFirstObjectByType<ToolManager>(FindObjectsInactive.Include);

            if (playerCamera == null)
                playerCamera = Camera.main;

            if (toolManager == null)
                Debug.LogError("RadialMenuController no encontró ToolManager en PlayerRoot.", this);
            if (playerCamera == null)
                Debug.LogError("RadialMenuController no encontró la cámara CenterEyeAnchor.", this);
            if (radialPartCanvas == null || radialPartPrefab == null)
                Debug.LogError("RadialMenuController necesita RadialPartCanvas y RadialPartPrefab.", this);
        }

        private void PositionInFrontOfPlayer()
        {
            Transform cameraTransform = playerCamera.transform;
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.ProjectOnPlane(cameraTransform.up, Vector3.up).normalized;

            Vector3 position = cameraTransform.position + forward * distanceFromCamera;
            position.y = cameraTransform.position.y + verticalOffset;
            radialPartCanvas.SetPositionAndRotation(position, Quaternion.LookRotation(forward));
        }

        private T GetOrAdd<T>() where T : Component
        {
            T component = GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }
    }
}
