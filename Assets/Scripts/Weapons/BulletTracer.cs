using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class BulletTracer : MonoBehaviour
{
    [SerializeField] List<Vector3> positions = new();
    Vector3 lastPosition;
    [SerializeField] int positionIndex;
    public float bulletSpeed;
    float distance;
    public void AddPosition(Vector3 position)
    {
        positions.Insert(Mathf.Max(0, positions.Count - 1), position);
    }
    private void Start()
    {
        StartCoroutine(LerpBullet());
    }
    IEnumerator LerpBullet()
    {
        float currentLerp = 0;
        lastPosition = transform.position;
        while (positionIndex < positions.Count)
        {
            distance = Vector3.Distance(lastPosition, positions[positionIndex]);
            while (currentLerp < 1)
            {
                transform.position = Vector3.Lerp(lastPosition, positions[positionIndex], currentLerp);
                currentLerp += Time.fixedDeltaTime * (bulletSpeed / distance);
                yield return null;
            }
            transform.position = positions[positionIndex];
            currentLerp = 0;
            lastPosition = transform.position;
            positionIndex++;
        }
    }
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < positions.Count; i++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(positions[i], 1);
        }
    }
}
