using GameData.Standards;
using Modules.Module01_CableMaking.Domain.Connector;
using Modules.Module01_CableMaking.Domain.Standards;
using Modules.Module01_CableMaking.Domain.Validation;
using TMPro;
using UnityEngine;

namespace Modules.Module01_CableMaking.Presentation
{
    public class RJ45PuzzleController : MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private Rj45Connector connector;

        [SerializeField]
        private CableStandard standard;

        [SerializeField]
        private TMP_Text resultText;

        private readonly StandardValidator validator = new();

        public void ValidatePuzzle()
        {
            ValidationResult result =
                validator.Validate(connector, standard);

            if (result.IsValid)
            {
                resultText.text = "Correcto";
            }
            else
            {
                resultText.text = "Incorrecto";
            }

            foreach (SlotValidation slot in result.Slots)
            {
                Debug.Log(
                    $"Slot {slot.SlotNumber} | " +
                    $"Esperado:{slot.ExpectedColor} | " +
                    $"Actual:{slot.CurrentColor}");
            }
        }
    }
}