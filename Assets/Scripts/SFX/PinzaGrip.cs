using UnityEngine;

public class PinzaGrip : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private OVRInput.Controller controller = OVRInput.Controller.RTouch;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        float trigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller);
        animator.SetFloat("Grip", trigger);
    }
}