using System.Collections;
using Systems.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public Transform cameraTransform;
    public FadeController fade;
    public GameObject locomotor;

    public InputActionProperty pauseAction;

    private bool isPaused = false;

    void Update()
    {
        var input = VRInputManager.Instance;
        if (input.PausePressed)
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
}