using UnityEngine;

namespace SFX
{
    public class DebrisSpawner : MonoBehaviour
    {
        [SerializeField] private TemporaryDebris debrisPrefab;
        [SerializeField] private Transform spawnPoint;

        public void Spawn()
        {
            if (debrisPrefab == null || spawnPoint == null)
                return;

            TemporaryDebris debris = Instantiate(
                debrisPrefab,
                spawnPoint.position,
                spawnPoint.rotation);

            debris.Release();
        }
    }
}