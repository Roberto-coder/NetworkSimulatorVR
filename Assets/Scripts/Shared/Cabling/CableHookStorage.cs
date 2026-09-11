using System.Collections;
using HPhysic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Shared.Cabling
{
    /// <summary>Holds a cable in a draped pose until either end is selected.</summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(PhysicCable))]
    public sealed class CableHookStorage : MonoBehaviour
    {
        private Rigidbody[] bodies;
        private bool[] originalKinematic;
        private XRGrabInteractable[] grabs;
        private bool released;
        public bool IsStored => !released;

        private void Awake()
        {
            bodies = GetComponentsInChildren<Rigidbody>(true);
            originalKinematic = new bool[bodies.Length];
            for (int i = 0; i < bodies.Length; i++)
            {
                originalKinematic[i] = bodies[i].isKinematic;
                bodies[i].isKinematic = true;
            }
            grabs = GetComponentsInChildren<XRGrabInteractable>(true);
        }

        private void OnEnable()
        {
            foreach (var grab in grabs)
            {
                grab.selectEntered.AddListener(Release);
                grab.selectExited.AddListener(RestoreReleasedEnd);
            }
        }

        private void OnDisable()
        {
            foreach (var grab in grabs)
            {
                grab.selectEntered.RemoveListener(Release);
                grab.selectExited.RemoveListener(RestoreReleasedEnd);
            }
        }

        private IEnumerator Start()
        {
            // PhysicCable builds its ordered point list in Start.
            yield return null;
            if (released) yield break;
            var points = GetComponent<PhysicCable>().Points;
            float step = Vector3.Distance(points[0].position, points[1].position);
            // Inverted U: every edge retains its original length to avoid spring preload.
            int middle = (points.Count - 1) / 2;
            float xStep = step * 0.22f;
            float yStep = Mathf.Sqrt(step * step - xStep * xStep);
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 local = new((i - middle) * xStep, -Mathf.Abs(i - middle) * yStep, 0);
                Vector3 position = transform.position + transform.rotation * local;
                var body = points[i].GetComponent<Rigidbody>();
                if (body != null) body.position = position;
                points[i].position = position;
            }
        }

        private void Release(SelectEnterEventArgs args)
        {
            if (released) return;
            released = true;
            for (int i = 0; i < bodies.Length; i++)
            {
                // XRI owns the selected endpoint's physics while held.
                var grab = bodies[i].GetComponent<XRGrabInteractable>();
                if (grab != null && grab.isSelected) continue;
                bodies[i].isKinematic = originalKinematic[i];
            }
        }

        private void RestoreReleasedEnd(SelectExitEventArgs args)
        {
            var body = args.interactableObject.transform.GetComponent<Rigidbody>();
            if (!released || body == null) return;
            int index = System.Array.IndexOf(bodies, body);
            if (index >= 0) body.isKinematic = originalKinematic[index];
        }
    }
}
