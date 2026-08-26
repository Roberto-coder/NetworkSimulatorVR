using Framework.Interaction.Tools;
using Framework.Interaction.Tools.Interfaces;
using Modules.Module01_CableMaking.Flow.Validation;
using UnityEngine;

namespace Modules.Module01_CableMaking.Interaction
{
    [RequireComponent(typeof(ToolInteractor))]
    public class TesterTool : InteractionTool<ITestable>
    {
        private ToolInteractor interactor;

        [SerializeField]
        private TesterAnimationController animationController;

        private TesterDockerController dockerController;

        private void Awake()
        {
            interactor = GetComponent<ToolInteractor>();
            dockerController = GetComponent<TesterDockerController>();
        }

        private void OnEnable()
        {
            interactor.InteractPressed += TryTest;
            SimulationManager.Instance?.FlowController.RegisterTesterDocker(dockerController);
        }

        private void OnDisable()
        {
            if (interactor != null)
                interactor.InteractPressed -= TryTest;

            SimulationManager.Instance?.FlowController.UnregisterTesterDocker(dockerController);
        }

        private void TryTest()
        {
            ModuleActionValidator validator =
                SimulationManager.Instance?.FlowController?.ActionValidator;

            if (validator != null &&
                !validator.TryValidate(ModuleActionType.ValidateCable))
            {
                return;
            }

            if (dockerController == null || !dockerController.AreBothConnected)
            {
                Debug.Log("Conecta ambos extremos del cable al tester antes de probarlo.");
                return;
            }

            if (!TryGetTarget(out ITestable testable))
                return;

            if (animationController != null)
                animationController.PlayTestAnimation();

            // Temporal
            // Cuando exista el sistema de validación simplemente reemplazarás la parte temporal por algo similar a:
            //
            // bool valid = validator.Validate(testable);
            //
            // if(valid)
            // {
            //     Debug.Log("Cable válido");
            // }
            // else
            // {
            //     Debug.Log("Cable inválido");
            // }
            Debug.Log($"[StripperTool] Intentando testear: {((Component)testable).name}", this);
            testable.Test();
        }
    }
}
