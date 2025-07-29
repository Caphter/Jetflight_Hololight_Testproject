using UnityEngine;

public class CrosshairTargetColliderCheck : MonoBehaviour
{
    public bool targetInCrosshair = false;
    [SerializeField] private SpriteRenderer crosshairSpriteRenderer;
    [SerializeField] private Color targetInCrosshairColor;
    [SerializeField] private Color defaultCrosshairColor;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            targetInCrosshair = true;
            crosshairSpriteRenderer.color = targetInCrosshairColor;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            targetInCrosshair = false;
            crosshairSpriteRenderer.color = defaultCrosshairColor;
        }
    }
}
