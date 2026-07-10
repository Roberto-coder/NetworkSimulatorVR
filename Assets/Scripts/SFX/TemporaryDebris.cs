using UnityEngine;

namespace SFX
{
    [RequireComponent(typeof(Rigidbody))]
    public class TemporaryDebris : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 5f;
        [SerializeField] private float randomTorque = 2f;
        [SerializeField] private float randomForce = 0.3f;

        private Rigidbody _rb;

        private void Awake() => _rb = GetComponent<Rigidbody>();

        public void Release()
        {
            transform.SetParent(null); // deja de seguir al cable
            _rb.isKinematic = false;
            _rb.AddTorque(Random.insideUnitSphere * randomTorque, ForceMode.Impulse);
            _rb.AddForce(Random.insideUnitSphere * randomForce, ForceMode.Impulse);
            //Destroy(gameObject, lifeTime);
            StartCoroutine(DisableAfterDelay());
        }
        
        private System.Collections.IEnumerator DisableAfterDelay()
        {
            yield return new WaitForSeconds(lifeTime);
            gameObject.SetActive(false); // en vez de Destroy
        }
    }
}