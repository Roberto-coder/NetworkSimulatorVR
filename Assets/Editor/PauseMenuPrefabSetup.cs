#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PauseMenuPrefabSetup
{
    private const string PrefabPath = "Assets/Prefabs/PlayerUi/PauseMenu.prefab";

    static PauseMenuPrefabSetup()
    {
        EditorApplication.delayCall += EnsurePrefabPanels;
    }

    [MenuItem("Network Simulator/UI/Actualizar Pause Menu")]
    public static void EnsurePrefabPanels()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            return;

        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            PauseMenuViewController controller = root.GetComponentInChildren<PauseMenuViewController>(true);
            if (controller == null)
            {
                Debug.LogError("PauseMenu.prefab no contiene PauseMenuViewController.");
                return;
            }

            Transform settings = controller.transform.Find("SettingsPanel");
            Transform confirmation = controller.transform.Find("ConfirmationPanel");
            if (settings == null || confirmation == null)
            {
                Debug.LogError("PauseMenu.prefab no contiene SettingsPanel y ConfirmationPanel.");
                return;
            }

            if (settings.childCount == 0 || confirmation.childCount == 0)
            {
                controller.BuildSecondaryPanelsIfNeeded();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log("PauseMenu actualizado con vistas Principal, Ajustes y Confirmación.");
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
#endif
