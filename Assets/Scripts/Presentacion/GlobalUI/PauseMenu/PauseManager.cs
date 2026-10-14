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
    private Transform playerOrigin;
    private Vector3 spawnFloorPosition;
    private Vector3 spawnForward;
    public bool CanReturnToSpawn => playerOrigin != null && cameraTransform != null;

    private void Start()
    {
        xrOrigin = cameraTransform != null ? cameraTransform.GetComponentInParent<XROrigin>() : null;
        var metaRig = cameraTransform != null ? cameraTransform.GetComponentInParent<OVRCameraRig>() : null;
        playerOrigin = xrOrigin != null ? xrOrigin.Origin.transform : metaRig != null ? metaRig.transform : null;
        if (playerOrigin == null) return;
        Transform origin = playerOrigin;
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
        if (!CanReturnToSpawn) return;
        foreach (var interactor in playerOrigin.GetComponentsInChildren<XRBaseInteractor>(true))
            if (interactor is not XRSocketInteractor && interactor.interactionManager != null)
                interactor.interactionManager.CancelInteractorSelection((IXRSelectInteractor)interactor);
        foreach (var gravity in playerOrigin.GetComponentsInChildren<GravityProvider>(true))
            gravity.ResetFallForce();
        foreach (var metaLocomotor in playerOrigin.GetComponentsInChildren<Oculus.Interaction.Locomotion.FirstPersonLocomotor>(true))
            metaLocomotor.Velocity = Vector3.zero;
        var metaControllers = playerOrigin.GetComponentsInChildren<Oculus.Interaction.Locomotion.CharacterController>(true);
        var metaPoses = new Pose[metaControllers.Length];
        for (int i = 0; i < metaControllers.Length; i++) metaPoses[i] = metaControllers[i].Pose;
        Vector3 previousPosition = playerOrigin.position;
        Quaternion previousRotation = playerOrigin.rotation;
        var controller = playerOrigin.GetComponent<CharacterController>();
        bool controllerEnabled = controller != null && controller.enabled;
        if (controllerEnabled) controller.enabled = false;
        try
        {
            // Preserve tracked head height, including seated/crouched posture.
            float headHeight = cameraTransform.position.y - playerOrigin.position.y;
            Vector3 destination = spawnFloorPosition + Vector3.up * headHeight;
            if (xrOrigin != null)
            {
                xrOrigin.MatchOriginUpCameraForward(Vector3.up, spawnForward);
                xrOrigin.MoveCameraToWorldLocation(destination);
            }
            else
            {
                Vector3 headForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
                if (headForward.sqrMagnitude > 0.001f)
                    playerOrigin.RotateAround(cameraTransform.position, Vector3.up,
                        Vector3.SignedAngle(headForward, spawnForward, Vector3.up));
                playerOrigin.position += destination - cameraTransform.position;
            }
            // Meta keeps a separate capsule pose; move it with the rig so it cannot pull the player back.
            Quaternion rotationDelta = playerOrigin.rotation * Quaternion.Inverse(previousRotation);
            for (int i = 0; i < metaControllers.Length; i++)
                metaControllers[i].SetPose(new Pose(
                    playerOrigin.position + rotationDelta * (metaPoses[i].position - previousPosition),
                    rotationDelta * metaPoses[i].rotation));
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
