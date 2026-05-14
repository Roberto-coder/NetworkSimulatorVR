using UnityEngine;
using HInteractions;

public class VRHandGrabber : MonoBehaviour, IObjectHolder
{
    public Transform handTransform;
    public float grabRange = 0.2f;
    public LayerMask interactableLayer;

    public Interactable SelectedObject { get; private set; }

    private Liftable currentCandidate;

    void Update()
    {
        DetectObject();

        bool grabbing = IsGrabbing();

        if (grabbing)
        {
            if (SelectedObject == null && currentCandidate != null)
            {
                TryGrab(currentCandidate);
            }
        }
        else
        {
            if (SelectedObject != null)
            {
                Release();
            }
        }
    }

    void DetectObject()
    {
        Collider[] hits = Physics.OverlapSphere(handTransform.position, grabRange, interactableLayer);

        currentCandidate = null;

        foreach (var hit in hits)
        {
            Liftable lift = hit.GetComponentInParent<Liftable>();
            if (lift != null)
            {
                currentCandidate = lift;
                break;
            }
        }
    }

    bool IsGrabbing()
    {
        return
            OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0.5f ||
            OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.5f;
    }

    public void TryGrab(Liftable obj)
    {
        SelectedObject = obj;
        obj.PickUp(this, gameObject.layer);
    }

    public void Release()
    {
        if (SelectedObject != null)
        {
            SelectedObject.GetComponent<Liftable>().Drop();
            SelectedObject = null;
        }
    }
}