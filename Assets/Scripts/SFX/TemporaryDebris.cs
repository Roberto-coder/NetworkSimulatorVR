using System.Collections;
using UnityEngine;

namespace SFX
{
    [RequireComponent(typeof(Rigidbody))]
    public class TemporaryDebris : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 5f;
        [SerializeField] private float randomTorque = 2f;
        [SerializeField] private float randomForce = 0.3f;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void Release()
        {
            rb.isKinematic = false;

            rb.AddTorque(
                Random.insideUnitSphere * randomTorque,
                ForceMode.Impulse);

            rb.AddForce(
                Random.insideUnitSphere * randomForce,
                ForceMode.Impulse);

            StartCoroutine(DisableAfterDelay());
        }

        private IEnumerator DisableAfterDelay()
        {
            yield return new WaitForSeconds(lifeTime);

            Destroy(gameObject);
        }
    }
}