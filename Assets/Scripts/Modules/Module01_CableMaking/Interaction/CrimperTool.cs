using Framework.Interaction.Tools.Interfaces;
using Framework.Interaction.Tools;
using UnityEngine;
namespace Modules.Module01_CableMaking.Interaction
{
    
    [RequireComponent(typeof(ToolInteractor))]
    public class CrimperTool : MonoBehaviour
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
            interactor.InteractPressed += TryCrimp;
        }

        private void OnDisable()
        {
            if (interactor != null)
                interactor.InteractPressed -= TryCrimp;
        }
        
        private void TryCrimp()
        {
            Collider[] hits = Physics.OverlapSphere(
                toolTip.position,
                radius,
                cableLayer);

            foreach (Collider hit in hits)
            {
                ICrimpable crimpable = hit.GetComponentInParent<ICrimpable>();

                if (crimpable == null)
                    continue;

                if (!crimpable.CanCrimp)
                    continue;

                crimpable.Crimp();
                return;
            }
        }
    }
}