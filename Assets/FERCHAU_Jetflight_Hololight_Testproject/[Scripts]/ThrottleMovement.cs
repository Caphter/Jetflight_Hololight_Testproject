using UnityEngine;

/// <summary>
/// Controls the way the throttle is moved by the user.
/// </summary>

public class ThrottleMovement : MonoBehaviour
{
    [SerializeField] private GameObject leftVRController;
    [SerializeField] private Transform throttleObject;
    [SerializeField] private GameObject minPositionReference;
    [SerializeField] private GameObject maxPositionReference;
    [SerializeField] private GameObject leftControllerMesh;

    private bool throttleGrabbed = false;
    

    void Update()
    {
        if (throttleGrabbed)
        {
            MoveThrottleAccordingToControllerPosition();
        }
    }

    private void MoveThrottleAccordingToControllerPosition()
    {
        // Projektionspunkte in der forward-Richtung der Throttle-Achse
        Vector3 minPos = minPositionReference.transform.position;
        Vector3 maxPos = maxPositionReference.transform.position;

        // Abstand zwischen Min- und Max-Referenzpunkten
        float totalDistance = Vector3.Distance(minPos, maxPos);

        // Projektion des Controllers entlang der forward-Richtung
        Vector3 controllerPos = leftVRController.transform.position;

        // Berechne den Wert entlang der Linie (min -> max) basierend auf der forward-Richtung
        float distanceToMin = Vector3.Dot(controllerPos - minPos, (maxPos - minPos).normalized);

        // Normalize auf den Bereich [0, 1] und dann auf die Zielskala [0, 65]
        float normalizedPosition = Mathf.Clamp01(distanceToMin / totalDistance);
        float targetRotationAngle = Mathf.Lerp(0f, 65f, normalizedPosition);

        // Setze die Rotation basierend auf dem berechneten Zielwinkel
        throttleObject.localRotation = Quaternion.Euler(targetRotationAngle, 0f, 0f);
    }

    public void GrabButtonPressed()
    {
        throttleGrabbed = true;
        leftControllerMesh.SetActive(false);
    }

    public void GrabButtonReleased()
    {
        throttleGrabbed = false;
        leftControllerMesh.SetActive(true);
    }
}
