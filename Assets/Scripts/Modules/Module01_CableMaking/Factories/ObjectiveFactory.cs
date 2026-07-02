using System;
using Core.Objectives;
using GameData.Objectives;
using Modules.Module01_CableMaking.Objectives;
using NetworkVR.Core.Objectives;

namespace Modules.Module01_CableMaking.Factories
{
    /// <summary>
    /// Crea los objetivos concretos correspondientes al modulo de armado de cable.
    /// </summary>
    public sealed class ObjectiveFactory
    {
        /// <summary>
        /// Crea un objetivo a partir de su configuracion.
        /// </summary>
        /// <param name="objectiveData">Datos que identifican el objetivo solicitado.</param>
        /// <returns>Una nueva instancia del objetivo correspondiente.</returns>
        /// <exception cref="ArgumentNullException">
        /// Se produce cuando <paramref name="objectiveData"/> es nulo.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Se produce cuando el identificador no corresponde a un objetivo conocido.
        /// </exception>
        public ObjectiveBase Create(ObjectiveData objectiveData)
        {
            if (objectiveData == null)
            {
                throw new ArgumentNullException(nameof(objectiveData));
            }

            return objectiveData.Id switch
            {
                "cut_cable" => new CutCableObjective(objectiveData),
                "strip_left_end" => new StripLeftEndObjective(objectiveData),
                "order_left_t568b" => new OrderLeftT568BObjective(objectiveData),
                "crimp_left_end" => new CrimpLeftEndObjective(objectiveData),
                "strip_right_end" => new StripRightEndObjective(objectiveData),
                "order_right_t568b" => new OrderRightT568BObjective(objectiveData),
                "crimp_right_end" => new CrimpRightEndObjective(objectiveData),
                "connect_tester" => new ConnectToTesterObjective(objectiveData),
                "validate_cable" => new ValidateCableObjective(objectiveData),
                _ => throw new ArgumentException(
                    $"No existe un objetivo asociado al identificador '{objectiveData.Id}'.",
                    nameof(objectiveData))
            };
        }
    }
}
