using System.Collections;
using Systems.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using Systems.Scenes;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public Transform cameraTransform;
    public FadeController fade;
    public GameObject locomotor;

    public InputActionProperty pauseAction;

    private bool isPaused = false;
    private XROrigin xrOrigin;
    private Vector3 spawnFloorPosition;
    private Vector3 spawnForward;
    public bool CanReturnToSpawn => xrOrigin != null && cameraTransform != null;

    private void Start()
    {
        xrOrigin = cameraTransform != null ? cameraTransform.GetComponentInParent<XROrigin>() : null;
        if (xrOrigin == null) return;
        Transform origin = xrOrigin.Origin.transform;
        spawnFloorPosition = new Vector3(cameraTransform.position.x, origin.position.y, cameraTransform.position.z);
        spawnForward = Vector3.ProjectOnPlane(origin.forward, Vector3.up).normalized;
        if (spawnForward.sqrMagnitude < 0.001f) spawnForward = Vector3.forward;
    }

    void Update()
    {
        var input = VRInputManager.Instance;
        if (SceneTransitionManager.IsLoading) return;
        if (input != null && input.PausePressed)
            TogglePause();
    }

    void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            locomotor.SetActive(false); 
            ShowMenu();
            StartCoroutine(fade.FadeIn());
            Time.timeScale = 0.0001f;;
        }
        else
        {
            locomotor.SetActive(true);
            StartCoroutine(UnpauseRoutine());
            HideMenu();
            Time.timeScale = 1;
        }
    }

    void ShowMenu()
    {
        pauseMenu.SetActive(true);
        pauseMenu.GetComponentInChildren<PauseMenuViewController>(true)?.ShowMain();

        pauseMenu.transform.position =
            cameraTransform.position + cameraTransform.forward * 2f;

        pauseMenu.transform.rotation =
            Quaternion.LookRotation(
                pauseMenu.transform.position - cameraTransform.position
            );
    }
    
    IEnumerator UnpauseRoutine()
    {
        yield return StartCoroutine(fade.FadeOut());
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    void HideMenu()
    {
        pauseMenu.SetActive(false);
    }

    public void ResumeFromMenu()
    {
        if (!isPaused)
            return;

        isPaused = false;
        if (locomotor != null)
            locomotor.SetActive(true);
        StartCoroutine(UnpauseRoutine());
    }

    public void ReturnToSpawn()
    {
        if (!CanReturnToSpawn || SceneTransitionManager.IsLoading) return;
        StopAllCoroutines();
        isPaused = false;
        if (locomotor != null) locomotor.SetActive(false);
        HideMenu();
        SceneTransitionManager.RunWithLoadingScreen(RestoreSpawnPose, () =>
        {
            if (this != null && locomotor != null) locomotor.SetActive(true);
        });
    }

    private void RestoreSpawnPose()
    {
        if (xrOrigin == null || cameraTransform == null) return;
        foreach (var interactor in xrOrigin.GetComponentsInChildren<XRBaseInteractor>(true))
            if (interactor is not XRSocketInteractor && interactor.interactionManager != null)
                interactor.interactionManager.CancelInteractorSelection((IXRSelectInteractor)interactor);
        foreach (var gravity in xrOrigin.GetComponentsInChildren<GravityProvider>(true))
            gravity.ResetFallForce();
        var controller = xrOrigin.GetComponent<CharacterController>();
        bool controllerEnabled = controller != null && controller.enabled;
        if (controllerEnabled) controller.enabled = false;
        try
        {
            // Preserve tracked head height, including seated/crouched posture.
            float headHeight = cameraTransform.position.y - xrOrigin.Origin.transform.position.y;
            xrOrigin.MatchOriginUpCameraForward(Vector3.up, spawnForward);
            xrOrigin.MoveCameraToWorldLocation(spawnFloorPosition + Vector3.up * headHeight);
        }
        finally
        {
            if (controllerEnabled && controller != null) controller.enabled = true;
        }
        if (fade != null)
        {
            if (fade.fadeImage != null)
            {
                Color color = fade.fadeImage.color;
                color.a = 0f;
                fade.fadeImage.color = color;
            }
        }
        Physics.SyncTransforms();
    }
}
