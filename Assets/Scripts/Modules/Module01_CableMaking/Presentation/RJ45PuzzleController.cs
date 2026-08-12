using System;
using Framework.Interaction.Tools.Interfaces;
using GameData.Standards;
using Modules.Module01_CableMaking.Domain.Cable;
using Modules.Module01_CableMaking.Domain.Connector;
using Modules.Module01_CableMaking.Domain.Standards;
using Modules.Module01_CableMaking.Domain.Validation;
using TMPro;
using UnityEngine;

namespace Modules.Module01_CableMaking.Presentation
{
    public class RJ45PuzzleController : MonoBehaviour,IOrdenable
    {
        [Header("References")]

        [SerializeField] private CableEnd end;

        [SerializeField]
        private Rj45Connector connector;

        [SerializeField]
        private CableStandard standard;

        [SerializeField]
        private TMP_Text resultText;

        private readonly StandardValidator validator = new();

        private CableStateController stateController;
        
        private void Awake()
        {
            stateController = GetComponentInParent<CableStateController>();
        }

        /// <summary>
        /// Configura la instancia reutilizable del puzzle para el extremo que
        /// esta activo. El controlador se inyecta para no depender de que el
        /// punto de spawn sea hijo del cable.
        /// </summary>
        public void Configure(CableEnd cableEnd, CableStateController controller)
        {
            end = cableEnd;
            stateController = controller;
        }
        
        public bool CanOrder => stateController != null && stateController.CanOrderRJ45(end);
        
        public void Order()
        {
            if (!CanOrder)
                return;
            // Aqui la condicion de que el cable este en el estado correcto para pelar, si no esta en ese estado no se puede pelar
            if (!stateController.TryAdvance(end, CableState.Rj45Ordered))
                return;
            // Aqui la condicion de que el cable este en el estado correcto para pelar, si no esta en ese estado no se puede pelar
            CableEvents.RaiseCableOrdered(end);
        }
        
        public void ValidatePuzzle()
        {
            if (connector == null || standard == null)
            {
                Debug.LogError("[RJ45Puzzle] Faltan referencias de Connector o CableStandard.", this);
                SetResult("Configuracion incompleta");
                return;
            }

            ValidationResult result =
                validator.Validate(connector, standard);

            if (result.IsValid)
            {
                SetResult("Correcto");
                Order();
            }
            else
            {
                SetResult("Incorrecto");
            }

            foreach (string error in result.Errors)
                Debug.LogWarning($"[RJ45Puzzle] {error}", this);

            foreach (SlotValidation slot in result.Slots)
            {
                Debug.Log(
                    $"Slot {slot.SlotNumber} | " +
                    $"Esperado:{slot.ExpectedColor} | " +
                    $"Actual:{slot.CurrentColor}");
            }
        }

        private void SetResult(string message)
        {
            if (resultText != null)
                resultText.text = message;
        }
    }
}
