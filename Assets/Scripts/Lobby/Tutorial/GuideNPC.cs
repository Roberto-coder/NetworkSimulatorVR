using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideNPC : MonoBehaviour
{
    [Header("Movimiento")]
    public List<Transform> waypoints;
    public float speed = 2f;
    public float rotationSpeed = 5f;

    [Header("Referencias")]
    public Transform player;

    private bool isMoving = false;

    // Mover a un punto específico
    public IEnumerator MoveToPoint(int index)
    {
        if (index >= waypoints.Count) yield break;

        isMoving = true;
        Transform target = waypoints[index];

        while (Vector3.Distance(transform.position, target.position) > 0.2f)
        {
            // Movimiento
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                speed * Time.deltaTime
            );

            // Rotación hacia dirección de movimiento
            Vector3 dir = (target.position - transform.position).normalized;
            if (dir != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    lookRot,
                    rotationSpeed * Time.deltaTime
                );
            }

            yield return null;
        }

        isMoving = false;
    }

    // 👉 Mirar al jugador (suave)
    public IEnumerator LookAtPlayer(float duration = 2f)
    {
        float t = 0;

        while (t < duration)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            if (dir != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    lookRot,
                    rotationSpeed * Time.deltaTime
                );
            }

            t += Time.deltaTime;
            yield return null;
        }
    }
}