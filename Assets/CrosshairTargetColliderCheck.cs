using UnityEngine;

public class CrosshairTargetColliderCheck : MonoBehaviour
{
    [SerializeField] private MissileManager missileManagerScript;
    [SerializeField] private SpriteRenderer crosshairSpriteRenderer;
    [SerializeField] private Color targetInCrosshairColor;
    [SerializeField] private Color defaultCrosshairColor;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            missileManagerScript.targetLocked = true;
            missileManagerScript.currentTarget = other.transform;
            crosshairSpriteRenderer.color = targetInCrosshairColor;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            missileManagerScript.targetLocked = false;
            missileManagerScript.currentTarget = null;
            crosshairSpriteRenderer.color = defaultCrosshairColor;
        }
    }
}
