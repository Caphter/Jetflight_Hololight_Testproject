using UnityEngine;

public class NoTargetingCollisionCheck : MonoBehaviour
{
    [Header("Collision Point")]
    public Vector3 lastCollisionPoint;
    [SerializeField] private Transform defaultPosition;

    private void OnTriggerStay(Collider other)
    {
        lastCollisionPoint = other.ClosestPoint(transform.position);
        Debug.DrawRay(lastCollisionPoint, Vector3.up * 5f, Color.red);
    }

    private void OnTriggerExit(Collider other)
    {
        lastCollisionPoint = defaultPosition.position;
    }
}