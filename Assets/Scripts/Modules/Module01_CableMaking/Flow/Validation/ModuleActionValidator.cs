using System;
using GameData.Objectives;
using Modules.Module01_CableMaking.Domain.Cable;

namespace Modules.Module01_CableMaking.Flow.Validation
{
    public enum ModuleActionType
    {
        Peel,
        PrepareOrder,
        Order,
        Crimp,
        ConnectTester,
        ValidateCable
    }

    public enum ModuleInteractionErrorType
    {
        WrongAction,
        WrongCableEnd,
        IncorrectWireOrder
    }

    public sealed class ModuleInteractionError
    {
        public ModuleInteractionError(
            ModuleInteractionErrorType type,
            string message)
        {
            Type = type;
            Message = message;
        }

        public ModuleInteractionErrorType Type { get; }
        public string Message { get; }
    }

    /// <summary>
    /// Valida una intención antes de que las mecánicas modifiquen el cable.
    /// </summary>
    public sealed class ModuleActionValidator
    {
        private readonly Func<ObjectiveData> getCurrentObjective;

        public ModuleActionValidator(Func<ObjectiveData> getCurrentObjective)
        {
            this.getCurrentObjective = getCurrentObjective
                ?? throw new ArgumentNullException(nameof(getCurrentObjective));
        }

        public event Action<ModuleInteractionError> ActionRejected;

        public bool TryValidate(
            ModuleActionType action,
            CableEnd? end = null)
        {
            ObjectiveData objective = getCurrentObjective();
            if (objective == null)
                return Reject(
                    ModuleInteractionErrorType.WrongAction,
                    "No hay una acción pendiente en este momento.");

            if (!MatchesAction(objective.Id, action))
            {
                return Reject(
                    ModuleInteractionErrorType.WrongAction,
                    $"Esa herramienta o acción no corresponde ahora. Objetivo actual: {objective.Title}.");
            }

            CableEnd? expectedEnd = GetExpectedEnd(objective.Id);
            if (end.HasValue && expectedEnd.HasValue && end != expectedEnd)
            {
                string expectedName = expectedEnd == CableEnd.Left
                    ? "izquierdo"
                    : "derecho";

                return Reject(
                    ModuleInteractionErrorType.WrongCableEnd,
                    $"Ese no es el extremo indicado. Trabaja primero el extremo {expectedName}.");
            }

            return true;
        }

        public void ReportIncorrectWireOrder()
        {
            Reject(
                ModuleInteractionErrorType.IncorrectWireOrder,
                "El orden de los conductores no coincide con la norma T568B. Revisa los colores e inténtalo nuevamente.");
        }

        private bool Reject(
            ModuleInteractionErrorType type,
            string message)
        {
            ActionRejected?.Invoke(new ModuleInteractionError(type, message));
            return false;
        }

        private static bool MatchesAction(
            string objectiveId,
            ModuleActionType action)
        {
            if (objectiveId.StartsWith("strip_", StringComparison.Ordinal))
                return action == ModuleActionType.Peel;

            if (objectiveId.StartsWith("order_", StringComparison.Ordinal))
            {
                return action == ModuleActionType.PrepareOrder ||
                       action == ModuleActionType.Order;
            }

            if (objectiveId.StartsWith("crimp_", StringComparison.Ordinal))
                return action == ModuleActionType.Crimp;

            return objectiveId switch
            {
                "connect_tester" => action == ModuleActionType.ConnectTester,
                "validate_cable" => action == ModuleActionType.ValidateCable,
                _ => false
            };
        }

        private static CableEnd? GetExpectedEnd(string objectiveId)
        {
            if (objectiveId.Contains("_left_", StringComparison.Ordinal) ||
                objectiveId.EndsWith("_left", StringComparison.Ordinal))
            {
                return CableEnd.Left;
            }

            if (objectiveId.Contains("_right_", StringComparison.Ordinal) ||
                objectiveId.EndsWith("_right", StringComparison.Ordinal))
            {
                return CableEnd.Right;
            }

            return null;
        }
    }
}
