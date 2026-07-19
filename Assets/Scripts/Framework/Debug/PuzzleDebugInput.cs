using UnityEngine;
using Framework.Interaction;
using Modules.Module01_CableMaking.Presentation;

namespace Framework.Debug
{
    public class PuzzleDebugInput : MonoBehaviour
    {
        [SerializeField]
        private RJ45PuzzleController controller;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                controller.ValidatePuzzle();
            }
        }
    }
}