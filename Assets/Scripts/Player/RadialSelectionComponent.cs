using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialSelectionComponent : MonoBehaviour
{
    [Header("UI")]
    public GameObject radialPartPrefab;
    public Transform radialPartCanvas;
    public float distanceFromCamera = 1.5f;

    [Header("Radial Settings")]
    [Range(2, 10)]
    public int numberOfRadialParts = 4;
    public float angleBetweenParts = 10f;

    [Header("Input")]
    public OVRInput.Button spawnButton;
    public Transform rightHandTransform;
    public Camera playerCamera;

    [Header("Tools")]
    public List<ToolData> tools;
    public ToolManager toolManager;

    private List<GameObject> spawnedParts = new List<GameObject>();
    private int currentIndex = -1;
    private bool menuActive = false;

    void Start()
    {
        radialPartCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (OVRInput.GetDown(spawnButton))
            ShowMenu();

        if (OVRInput.Get(spawnButton) && menuActive)
            UpdateSelection();

        if (OVRInput.GetUp(spawnButton) && menuActive)
            ConfirmSelection();
    }

    void ShowMenu()
    {
        menuActive = true;
        currentIndex = -1;

        Transform cam = playerCamera.transform;

        // Posición frente al usuario
        radialPartCanvas.position = cam.position + cam.forward * distanceFromCamera;

        // Rotación estable
        Vector3 forward = cam.forward;
        forward.y = 0;
        radialPartCanvas.rotation = Quaternion.LookRotation(forward);

        radialPartCanvas.gameObject.SetActive(true);

        SpawnParts();
    }

    void ConfirmSelection()
    {
        if (currentIndex >= 0 && currentIndex < tools.Count)
        {
            toolManager.EquipTool(tools[currentIndex]);
        }

        radialPartCanvas.gameObject.SetActive(false);
        menuActive = false;
    }

    void UpdateSelection()
    {
        if (spawnedParts.Count == 0) return;

        Vector3 dir = rightHandTransform.position - radialPartCanvas.position;
        Vector3 projected = Vector3.ProjectOnPlane(dir, radialPartCanvas.forward);

        float angle = Vector3.SignedAngle(radialPartCanvas.up, projected, -radialPartCanvas.forward);
        if (angle < 0) angle += 360f;

        currentIndex = Mathf.Clamp(
            (int)(angle * numberOfRadialParts / 360f),
            0,
            numberOfRadialParts - 1
        );

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
        foreach (var part in spawnedParts)
            if (part != null) Destroy(part);

        spawnedParts.Clear();

        float step = 360f / numberOfRadialParts;
        float start = -step / 2f - angleBetweenParts / 2f;

        for (int i = 0; i < numberOfRadialParts; i++)
        {
            float angle = start - i * step;

            GameObject part = Instantiate(radialPartPrefab, radialPartCanvas);
            part.transform.localPosition = Vector3.zero;
            part.transform.localEulerAngles = new Vector3(0, 0, angle);

            Image img = part.GetComponent<Image>();

            if (img != null)
            {
                img.type = Image.Type.Filled;
                img.fillMethod = Image.FillMethod.Radial360;
                img.fillAmount = (step - angleBetweenParts) / 360f;
            }

            // ICONO (seguro)
            if (i < tools.Count && tools[i] != null)
            {
                Transform icon = part.transform.Find("Icon");

                if (icon != null)
                {
                    Image iconImg = icon.GetComponent<Image>();

                    if (iconImg != null && tools[i].icon != null)
                    {
                        iconImg.sprite = tools[i].icon;
                        iconImg.preserveAspect = true;
                    }
                }
            }

            spawnedParts.Add(part);
        }
    }
}