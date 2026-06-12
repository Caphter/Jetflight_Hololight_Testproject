using UnityEngine;

public class MirrorMainCameraPosition : MonoBehaviour
{
    [SerializeField] private Transform mainCameraTransform;
    [SerializeField] private TargetingSystem targetingSystemScript;

    private void Update()
    {
        if(!targetingSystemScript.triggerTargetingActive)
        {
            this.transform.position = mainCameraTransform.position;
            this.transform.rotation = mainCameraTransform.rotation;
        }
    }
}
