using UnityEngine;

public class CrosshairTargetColliderCheck : MonoBehaviour
{
    // Die Referenz auf dein normales Fadenkreuz
    [SerializeField] private SpriteRenderer normalCrosshairSpriteRenderer;
    [SerializeField] private TargetingSystem targetingSystemScript;



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
        }
    }
}