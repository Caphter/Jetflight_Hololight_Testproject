using UnityEngine;

public class CrosshairTargetColliderCheck : MonoBehaviour
{
    [Header("Targeting:")]
    [SerializeField] private TargetingSystem targetingSystemScript;
    [SerializeField] private Transform defaultTargetPoint;

    [Header("Raycast Settings:")]
    [SerializeField] private float sphereCastRadius = 0.5f;
    [SerializeField] private float raycastDistance = 200f;
    [SerializeField] private LayerMask raycastLayers; // NEU: Variable umbenannt

    public Vector3 currentCollidingPoint;

    private void Start()
    {
        currentCollidingPoint = defaultTargetPoint.position;
    }

    private void Update()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.forward;

        if (Physics.SphereCast(rayOrigin, sphereCastRadius, rayDirection, out hit, raycastDistance, raycastLayers))
        {
            currentCollidingPoint = hit.point;

            if (hit.collider.CompareTag("Target"))
            {
                targetingSystemScript.SetTargetLock(hit.collider.transform);
            }
            else
            {
                targetingSystemScript.ReleaseTargetLock();
            }
        }
        else
        {
            targetingSystemScript.ReleaseTargetLock();
            currentCollidingPoint = rayOrigin + rayDirection * raycastDistance;
        }
    }
}