using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Systems.Scenes
{
    /// <summary>
    /// Transición global apta para VR. Mantiene un panel estable frente a la
    /// cabeza, carga de forma asíncrona y evita solicitudes simultáneas.
    /// </summary>
    public sealed class SceneTransitionManager : MonoBehaviour
    {
        private const float FadeDuration = 0.3f;
        private const float MinimumVisibleTime = 0.55f;
        private const float CanvasDistance = 0.5f;

        public static SceneTransitionManager Instance { get; private set; }
        public static bool IsLoading => Instance != null && Instance.isLoading;

        private Canvas loadingCanvas;
        private CanvasGroup canvasGroup;
        private Image progressFill;
        private TMP_Text progressText;
        private Transform head;
        private bool isLoading;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateGlobalInstance()
        {
            if (Instance != null) return;
            GameObject manager = new GameObject("SceneTransitionManager");
            manager.AddComponent<SceneTransitionManager>();
        }

        public static void LoadScene(string sceneName)
        {
            EnsureInstance();
            Instance.RequestLoad(sceneName);
        }

        public static void LoadScene(int buildIndex)
        {
            EnsureInstance();
            Instance.RequestLoad(buildIndex);
        }

        private static void EnsureInstance()
        {
            if (Instance != null) return;
            GameObject manager = new GameObject("SceneTransitionManager");
            manager.AddComponent<SceneTransitionManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildLoadingCanvas();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            Instance = null;
        }

        private void LateUpdate()
        {
            if (canvasGroup == null || canvasGroup.alpha <= 0f)
                return;

            ResolveHead();
            PositionCanvas();
        }

        private void RequestLoad(string sceneName)
        {
            if (isLoading) return;
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("No se puede cargar una escena sin nombre.", this);
                return;
            }
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"La escena '{sceneName}' no existe o no está habilitada en Build Settings.", this);
                return;
            }
            StartCoroutine(LoadRoutine(() => SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single)));
        }

        private void RequestLoad(int buildIndex)
        {
            if (isLoading) return;
            if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError($"Índice de escena no válido: {buildIndex}.", this);
                return;
            }
            StartCoroutine(LoadRoutine(() => SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single)));
        }

        private IEnumerator LoadRoutine(Func<AsyncOperation> createOperation)
        {
            isLoading = true;
            Time.timeScale = 1f;
            SetProgress(0f);
            ResolveHead();
            PositionCanvas();
            canvasGroup.blocksRaycasts = true;

            yield return Fade(0f, 1f, FadeDuration);
            float visibleSince = Time.realtimeSinceStartup;
            AsyncOperation operation = createOperation();
            if (operation == null)
            {
                Debug.LogError("Unity no pudo crear la operación de carga.", this);
                yield return Fade(1f, 0f, FadeDuration);
                canvasGroup.blocksRaycasts = false;
                isLoading = false;
                yield break;
            }
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                SetProgress(Mathf.Clamp01(operation.progress / 0.9f));
                yield return null;
            }

            SetProgress(1f);
            float remaining = MinimumVisibleTime - (Time.realtimeSinceStartup - visibleSince);
            if (remaining > 0f)
                yield return new WaitForSecondsRealtime(remaining);

            operation.allowSceneActivation = true;
            while (!operation.isDone)
                yield return null;

            // Da tiempo a que el nuevo rig registre CenterEyeAnchor/Camera.main.
            yield return null;
            yield return null;
            head = null;
            ResolveHead();
            PositionCanvas();
            yield return Fade(1f, 0f, FadeDuration);

            canvasGroup.blocksRaycasts = false;
            isLoading = false;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;
            canvasGroup.alpha = from;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            canvasGroup.alpha = to;
        }

        private void HandleSceneLoaded(Scene _, LoadSceneMode __)
        {
            head = null;
            ResolveHead();
        }

        private void ResolveHead()
        {
            if (head != null && head.gameObject.activeInHierarchy)
                return;

            GameObject centerEye = GameObject.Find("CenterEyeAnchor");
            head = centerEye != null ? centerEye.transform : Camera.main?.transform;
            if (loadingCanvas != null)
                loadingCanvas.worldCamera = head != null ? head.GetComponent<Camera>() : Camera.main;
        }

        private void PositionCanvas()
        {
            if (head == null || loadingCanvas == null)
                return;

            Transform canvasTransform = loadingCanvas.transform;
            canvasTransform.SetPositionAndRotation(
                head.position + head.forward * CanvasDistance,
                head.rotation);
        }

        private void SetProgress(float value)
        {
            value = Mathf.Clamp01(value);
            if (progressFill != null)
            {
                RectTransform rect = progressFill.rectTransform;
                rect.anchorMax = new Vector2(value, 1f);
            }
            if (progressText != null)
                progressText.text = $"CARGANDO  {Mathf.RoundToInt(value * 100f)}%";
        }

        private void BuildLoadingCanvas()
        {
            BuildLoadingCanvasFromPrefab();

            // Implementación anterior conservada como referencia. Ya no se usa porque
            // la interfaz se instancia desde Assets/Prefabs/UI_Components/GlobalUI/LoadingCanvas.prefab.
            // BuildLoadingCanvasFromCode();
        }

        private void BuildLoadingCanvasFromPrefab()
        {
            SceneTransitionSettings settings = Resources.Load<SceneTransitionSettings>("SceneTransitionSettings");
            if (settings == null || settings.LoadingCanvasPrefab == null)
            {
                Debug.LogError("Falta SceneTransitionSettings o su LoadingCanvasPrefab.", this);
                return;
            }

            GameObject canvasObject = Instantiate(settings.LoadingCanvasPrefab, transform, false);
            canvasObject.name = "LoadingCanvas";
            loadingCanvas = canvasObject.GetComponent<Canvas>();
            if (loadingCanvas == null)
            {
                Debug.LogError("LoadingCanvas.prefab necesita un componente Canvas en la raíz.", canvasObject);
                return;
            }

            loadingCanvas.renderMode = RenderMode.WorldSpace;
            loadingCanvas.overrideSorting = true;
            loadingCanvas.sortingOrder = short.MaxValue;

            RectTransform canvasRect = (RectTransform)canvasObject.transform;
            canvasRect.sizeDelta = new Vector2(1600f, 900f);
            canvasRect.localScale = Vector3.one * 0.001f;

            canvasGroup = canvasObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = canvasObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            RectTransform backgroundRect = FindRect(canvasRect, "Background");
            Image background = EnsureImage(backgroundRect, new Color32(3, 12, 24, 255));
            Stretch(background.rectTransform);

            RectTransform logoRect = FindRect(canvasRect, "Logo");
            if (logoRect != null)
            {
                SetRect(logoRect, new Vector2(0.5f, 0.5f), new Vector2(0f, 105f), new Vector2(320f, 180f));
                Image logo = logoRect.GetComponent<Image>();
                if (logo != null)
                {
                    logo.preserveAspect = true;
                    logo.raycastTarget = false;
                }
            }

            progressText = FindRect(canvasRect, "LoadingText")?.GetComponent<TMP_Text>();
            if (progressText != null)
            {
                progressText.text = "CARGANDO  0%";
                progressText.fontSize = 25f;
                progressText.fontStyle = FontStyles.Bold;
                progressText.color = new Color32(242, 247, 252, 255);
                progressText.alignment = TextAlignmentOptions.Center;
                progressText.raycastTarget = false;
                SetRect(progressText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -88f), new Vector2(600f, 45f));
            }

            RectTransform progressRect = FindRect(canvasRect, "Progressbar");
            Image progressBackground = EnsureImage(progressRect, new Color32(14, 48, 81, 255));
            SetRect(progressBackground.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -35f), new Vector2(760f, 28f));
            progressFill = CreateImage(progressBackground.rectTransform, "ProgressFill", new Color32(52, 200, 255, 255));
            progressFill.rectTransform.anchorMin = Vector2.zero;
            progressFill.rectTransform.anchorMax = new Vector2(0f, 1f);
            progressFill.rectTransform.offsetMin = new Vector2(4f, 4f);
            progressFill.rectTransform.offsetMax = new Vector2(-4f, -4f);
            progressFill.rectTransform.pivot = new Vector2(0f, 0.5f);
        }

        private static RectTransform FindRect(Transform root, string childName)
        {
            Transform child = root.Find(childName);
            return child as RectTransform;
        }

        private static Image EnsureImage(RectTransform rect, Color color)
        {
            if (rect == null)
                throw new InvalidOperationException("El prefab de carga no tiene todos los objetos requeridos.");

            if (rect.GetComponent<CanvasRenderer>() == null)
                rect.gameObject.AddComponent<CanvasRenderer>();
            Image image = rect.GetComponent<Image>();
            if (image == null)
                image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private void BuildLoadingCanvasFromCode()
        {
            GameObject canvasObject = new GameObject("LoadingCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(CanvasGroup));
            canvasObject.transform.SetParent(transform, false);
            loadingCanvas = canvasObject.GetComponent<Canvas>();
            loadingCanvas.renderMode = RenderMode.WorldSpace;
            loadingCanvas.overrideSorting = true;
            loadingCanvas.sortingOrder = short.MaxValue;

            RectTransform canvasRect = (RectTransform)canvasObject.transform;
            canvasRect.sizeDelta = new Vector2(1600f, 900f);
            canvasRect.localScale = Vector3.one * 0.001f;
            canvasGroup = canvasObject.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            Image background = CreateImage(canvasRect, "Background", new Color32(3, 12, 24, 255));
            Stretch(background.rectTransform);

            TMP_Text title = CreateText(canvasRect, "Title", "NETWORK SIMULATOR VR", 62,
                new Color32(52, 200, 255, 255), FontStyles.Bold);
            SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 115f), new Vector2(1100f, 90f));

            TMP_Text subtitle = CreateText(canvasRect, "Subtitle", "Preparando laboratorio…", 30,
                new Color32(183, 201, 216, 255), FontStyles.Normal);
            SetRect(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 42f), new Vector2(1000f, 55f));

            Image progressBackground = CreateImage(canvasRect, "ProgressBackground", new Color32(14, 48, 81, 255));
            SetRect(progressBackground.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -35f), new Vector2(760f, 28f));

            progressFill = CreateImage(progressBackground.rectTransform, "ProgressFill", new Color32(52, 200, 255, 255));
            progressFill.rectTransform.anchorMin = Vector2.zero;
            progressFill.rectTransform.anchorMax = new Vector2(0f, 1f);
            progressFill.rectTransform.offsetMin = new Vector2(4f, 4f);
            progressFill.rectTransform.offsetMax = new Vector2(-4f, -4f);
            progressFill.rectTransform.pivot = new Vector2(0f, 0.5f);

            progressText = CreateText(canvasRect, "ProgressText", "CARGANDO  0%", 25,
                new Color32(242, 247, 252, 255), FontStyles.Bold);
            SetRect(progressText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -88f), new Vector2(600f, 45f));
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            child.transform.SetParent(parent, false);
            Image image = child.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TMP_Text CreateText(Transform parent, string name, string value, float size, Color color, FontStyles style)
        {
            GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            child.transform.SetParent(parent, false);
            TMP_Text text = child.GetComponent<TMP_Text>();
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            text.enableWordWrapping = false;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
