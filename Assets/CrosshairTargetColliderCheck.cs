using UnityEngine;

public class CrosshairTargetColliderCheck : MonoBehaviour
{
    // Die Referenz auf dein normales Fadenkreuz
    [SerializeField] private SpriteRenderer normalCrosshairSpriteRenderer;
    [SerializeField] private TargetingSystem targetingSystemScript;
    [SerializeField] private Transform defaultTargetPoint;

    // Die Public Variable, die den aktuellen Kollisionspunkt speichert
    public Transform currentCollidingPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            targetingSystemScript.SetTargetLock(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            targetingSystemScript.ReleaseTargetLock();

            currentCollidingPoint = defaultTargetPoint;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            currentCollidingPoint = other.transform;
        }
    }
}