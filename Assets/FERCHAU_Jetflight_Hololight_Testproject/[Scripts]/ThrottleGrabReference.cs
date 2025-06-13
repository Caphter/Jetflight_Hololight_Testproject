using UnityEngine;

public class ThrottleGrabReference : MonoBehaviour
{
    [SerializeField] private GameObject originalThrottleObject;

    public void ReleaseThrottleGrabReferenceObject()
    {
        this.transform.localPosition = originalThrottleObject.transform.localPosition;
        this.transform.localRotation = originalThrottleObject.transform.localRotation;

    }
}
