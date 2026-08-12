using UnityEngine;

namespace Framework.Spawning
{
    public class ObjectSpawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject prefab;

        [SerializeField]
        private Transform spawnPoint;
        
        [SerializeField]
        private bool destroyOnDespawn = true;

        private GameObject instance;

        public GameObject CurrentInstance => instance;

        public bool HasInstance => instance != null;

        public void Spawn()
        {
            if (instance != null)
                return;

            instance = Instantiate(
                prefab,
                spawnPoint.position,
                spawnPoint.rotation,
                spawnPoint);
        }

        public void Despawn()
        {
            if (instance == null)
                return;

            if (destroyOnDespawn)
                Destroy(instance);
            else
                instance.SetActive(false);

            instance = null;
        }

        public void Respawn()
        {
            Despawn();
            Spawn();
        }
    }
}