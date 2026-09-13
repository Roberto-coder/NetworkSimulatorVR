using System;
using GameData.Objectives;
using Modules.Module02_RackInstallation.Objectives;

namespace Modules.Module02_RackInstallation.Factories
{
    /// <summary>Centraliza la creación; los nueve objetivos comparten su ciclo de vida.</summary>
    public sealed class Module02ObjectiveFactory
    {
        public Module02Objective Create(ObjectiveData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.Id))
                throw new ArgumentException("El objetivo necesita datos y un ID.", nameof(data));
            // Se preservan también definiciones antiguas usadas para probar mecánicas aisladas.
            return new Module02Objective(data);
        }
    }
}
