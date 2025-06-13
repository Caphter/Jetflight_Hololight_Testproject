using UnityEngine;
using System.Collections;

/// <summary>
/// This Unity script controls a VR joystick’s rotation based on the right controller’s position, constraining it within maximum X and Z angles, and smoothly returns it to its initial rotation when released. 
/// It uses Oculus input for grab detection, toggles the controller mesh visibility, and calculates rotation from positional offset with scaling and circular limits.
/// </summary>

public class JoystickConstraintLogic : MonoBehaviour
{
    [SerializeField] private GameObject joystickObject;
    [SerializeField] private GameObject joystickObjectInvisible;
    [SerializeField] private Transform originJoystickTransform;
    [SerializeField] private GameObject rightControllerMesh;
    [SerializeField] private float returnLerpSpeed = 5f;

    private bool joystickGrabbed = false;
    private Coroutine returnToCenterRoutine;
    private Quaternion joystickInitialRotation;

    [SerializeField] private float maxXRotation = 30f; // Maximale Rotation auf der X-Achse
    [SerializeField] private float maxZRotation = 30f; // Maximale Rotation auf der Z-Achse
    [SerializeField] private float xRotationScaling = 2f; // Skalierung der X-Rotationsstärke
    [SerializeField] private float zRotationScaling = 2f; // Skalierung der Z-Rotationsstärke

    private void Start()
    {
        joystickInitialRotation = joystickObject.transform.localRotation;

    }

    private void Update()
    {
        // Wenn der Joystick mit dem rechten Controller gepackt wird und sich der Controller im Trigger des Joysticks befindet
        if (joystickGrabbed)
        {
            CalculateJoystickRotation();
        }
        else
        {
            // Zurücksetzen der unsichtbaren Joystick-Position
            joystickObjectInvisible.transform.position = originJoystickTransform.position;
            joystickObjectInvisible.transform.rotation = originJoystickTransform.rotation;
        }
    }

    private void CalculateJoystickRotation()
    {
        // Berechne die Positionsänderung im lokalen Koordinatensystem des Joysticks
        Vector3 localPositionDelta = originJoystickTransform.InverseTransformDirection(joystickObjectInvisible.transform.position - originJoystickTransform.position);

        // Konvertiere lokale Positionsänderung in Rotation, verstärkt durch Skalierungsvariablen
        float xRotation = localPositionDelta.z * maxXRotation * xRotationScaling;
        float zRotation = -localPositionDelta.x * maxZRotation * zRotationScaling;

        // Berechne den Abstand (Radius) der Rotation von der Mitte
        float rotationMagnitude = Mathf.Sqrt(xRotation * xRotation + zRotation * zRotation);

        // Begrenze die Rotation auf einen maximalen Radius (Kreisbegrenzung)
        if (rotationMagnitude > maxXRotation)
        {
            float scale = maxXRotation / rotationMagnitude;
            xRotation *= scale;
            zRotation *= scale;
        }

        // Setze Rotation auf das sichtbare Joystick-Objekt
        joystickObject.transform.localRotation = Quaternion.Euler(xRotation, 0f, zRotation);
    }

    public void GrabButtonPressed()
    {
        joystickGrabbed = true;
        rightControllerMesh.SetActive(false);

        if (returnToCenterRoutine != null)
        {
            StopCoroutine(returnToCenterRoutine);
        }
    }

    public void GrabButtonReleased()
    {
        joystickGrabbed = false;
        rightControllerMesh.SetActive(true);
        returnToCenterRoutine = StartCoroutine(ReturnToCenter());
    }

    private IEnumerator ReturnToCenter()
    {
        while (!joystickGrabbed && Quaternion.Angle(joystickObject.transform.localRotation, joystickInitialRotation) > 0.1f)
        {
            joystickObject.transform.localRotation = Quaternion.Lerp(joystickObject.transform.localRotation, joystickInitialRotation, returnLerpSpeed * Time.deltaTime);
            yield return null;
        }

        joystickObject.transform.localRotation = joystickInitialRotation;
        returnToCenterRoutine = null;
    }
}