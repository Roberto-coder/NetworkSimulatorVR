using UnityEngine;

namespace Presentacion.Tutorial
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class DialogueCanvasSorting : MonoBehaviour
    {
        [SerializeField] private int sortingOrder = 30;

        private void Awake()
        {
            Canvas canvas = GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingLayerID = SortingLayer.NameToID("Default");
            canvas.sortingOrder = sortingOrder;
        }
    }
}
