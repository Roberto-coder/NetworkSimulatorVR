using UnityEngine;

namespace Modules.Lobby.Tutorial
{
    public class FloatingEffect : MonoBehaviour
    {
        public float amplitude = 0.2f;
        public float frequency = 1f;
        public float rotationAmount = 10f;

        private float initialY;

        void Start()
        {
            initialY = transform.position.y; // 👈 usar WORLD
        }

        void Update()
        {
            float offset = Mathf.Sin(Time.time * frequency) * amplitude;

            Vector3 pos = transform.position;
            pos.y = initialY + offset; // 👈 solo modifica Y

            transform.position = pos;

            transform.rotation = Quaternion.Euler(
                0,
                Mathf.Sin(Time.time * frequency) * rotationAmount,
                0
            );
        }
    }
}