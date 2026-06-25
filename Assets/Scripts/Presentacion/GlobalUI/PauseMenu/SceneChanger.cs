using UnityEngine;
using UnityEngine.SceneManagement; // Es necesario incluir esta librería [citation:3]

public class SceneChanger : MonoBehaviour
{
    // Este método se conectará al botón. Recibe el número de la escena a cargar.
    public void LoadSceneByIndex(int sceneIndex)
    {
        // Verifica si el índice proporcionado es válido.
        // SceneManager.sceneCountInBuildSettings te da el total de escenas en tu Build Settings.
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            // Carga la escena usando su número de índice [citation:2][citation:5]
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogError("Índice de escena no válido: " + sceneIndex + ". Asegúrate de que la escena esté en Build Settings.");
        }
    }
}