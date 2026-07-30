using Framework.Interaction.Tools;
using Framework.Interaction.Tools.Interfaces;
using UnityEngine;

namespace Modules.Module01_CableMaking.Interaction
{
    [RequireComponent(typeof(ToolInteractor))]
    public class TesterTool: MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private Transform toolTip;
        [SerializeField] private float radius = 0.015f;
        [SerializeField] private LayerMask cableLayer;

        private ToolInteractor interactor;

        private void Awake()
        {
            interactor = GetComponent<ToolInteractor>();
        }

        private void OnEnable()
        {
            interactor.InteractPressed += TryTest;
        }

        private void OnDisable()
        {
            if (interactor != null)
                interactor.InteractPressed -= TryTest;
        }

        private void TryTest()
        {
            Collider[] hits = Physics.OverlapSphere(
                toolTip.position,
                radius,
                cableLayer);

            foreach (Collider hit in hits)
            {
                ITestable testable = hit.GetComponentInParent<ITestable>();

                if (testable == null)
                    continue;

                if (!testable.CanTest)
                    continue;

                testable.Test();
                return;
            }
        }
    }
}