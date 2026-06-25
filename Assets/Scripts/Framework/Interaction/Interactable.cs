// Clase base para cualquier objeto interactuable.
// Gestiona el estado de selección y expone si debe mostrarse el indicador de interacción.

using NaughtyAttributes;
using UnityEngine;

namespace Framework.Interaction
{
    [DisallowMultipleComponent]
    public class Interactable : MonoBehaviour
    {
        [field: SerializeField] public bool ShowPointerOnInterract { get; private set; } = true;

        [field: SerializeField, ReadOnly] public bool IsSelected { get; private set; }

        protected virtual void Awake()
        {
            Deselect();
        }

        public virtual void Select()
        {
            IsSelected = true;
        }

        public virtual void Deselect()
        {
            IsSelected = false;
        }
    }
}