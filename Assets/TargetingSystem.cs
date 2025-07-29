using UnityEngine;
using UnityEngine.InputSystem;

public class TargetingSystem : MonoBehaviour
{
    [SerializeField] private GameObject hmdCrosshairParent;
    [SerializeField] private GameObject triggerTargetingCrosshairParent;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject leftController; // Referenz auf den linken Controller

    [SerializeField] private InputActionReference triggerLeftPressedButton;
    [SerializeField] private InputActionReference primaryRightPressedButton;

    [SerializeField] private float sensitivity = 0.1f; // Sensitivität der Fadenkreuzbewegung
    [SerializeField] private float maxVerticalAngle = 45f; // Maximaler vertikaler Winkel
    [SerializeField] private float maxHorizontalAngle = 45f; // Maximaler horizontaler Winkel

    private bool triggerTargetingActive = false;
    private bool targetingSystemActive = false;
    private Vector3 initialCrosshairPosition; // Position des Fadenkreuzes beim Start
    private Vector3 lastCrosshairPosition; // Letzte Position des Fadenkreuzes beim Trigger-Druck
    private Quaternion initialControllerRotation;

    private void Start()
    {
        triggerLeftPressedButton.action.Enable();
        triggerLeftPressedButton.action.performed += OnTriggerLeftPressed;
        triggerLeftPressedButton.action.canceled += OnTriggerLeftReleased;

        primaryRightPressedButton.action.Enable();
        primaryRightPressedButton.action.performed += OnPrimaryRightPressed;

        // Speichere die initiale Position des Fadenkreuzes beim Start
        initialCrosshairPosition = crosshair.transform.localPosition;
        lastCrosshairPosition = initialCrosshairPosition; // Setze Anfangsreferenz
    }

    private void OnDestroy()
    {
        if (triggerLeftPressedButton != null)
        {
            triggerLeftPressedButton.action.performed -= OnTriggerLeftPressed;
            triggerLeftPressedButton.action.canceled -= OnTriggerLeftReleased;

            primaryRightPressedButton.action.performed -= OnPrimaryRightPressed;
        }
    }

    private void Update()
    {
        if (targetingSystemActive && triggerTargetingActive)
        {
            // Berechne die Rotation des Controllers relativ zur Ausgangsrotation
            Quaternion controllerRotation = leftController.transform.rotation;
            Vector3 rotationDelta = (controllerRotation * Quaternion.Inverse(initialControllerRotation)).eulerAngles;

            // Normalisiere die Euler-Winkel (0 bis 360 -> -180 bis 180)
            float deltaX = NormalizeAngle(rotationDelta.x); // X-Rotation (Neigung)
            float deltaY = NormalizeAngle(rotationDelta.y); // Y-Rotation (Drehung)

            // Berechne die neue Position des Fadenkreuzes
            Vector3 newCrosshairPosition = lastCrosshairPosition;
            newCrosshairPosition.y += deltaX * sensitivity; // Vertikale Bewegung basierend auf X-Rotation
            newCrosshairPosition.x += deltaY * sensitivity; // Horizontale Bewegung basierend auf Y-Rotation (invertiert)

            // Begrenze die Bewegung des Fadenkreuzes
            newCrosshairPosition.y = Mathf.Clamp(newCrosshairPosition.y,
                lastCrosshairPosition.y - maxVerticalAngle * sensitivity,
                lastCrosshairPosition.y + maxVerticalAngle * sensitivity);
            newCrosshairPosition.x = Mathf.Clamp(newCrosshairPosition.x,
                lastCrosshairPosition.x - maxHorizontalAngle * sensitivity,
                lastCrosshairPosition.x + maxHorizontalAngle * sensitivity);

            // Setze die neue Position des Fadenkreuzes
            crosshair.transform.localPosition = newCrosshairPosition;
        }
    }

    private void OnTriggerLeftPressed(InputAction.CallbackContext context)
    {
        if (targetingSystemActive)
        {
            triggerTargetingActive = true;
            crosshair.transform.parent = triggerTargetingCrosshairParent.transform;
            // Speichere die aktuelle Position des Fadenkreuzes als Referenz
            lastCrosshairPosition = crosshair.transform.localPosition;
            // Speichere die aktuelle Controller-Rotation als Referenz
            initialControllerRotation = leftController.transform.rotation;
        }
    }

    private void OnTriggerLeftReleased(InputAction.CallbackContext context)
    {
        if (targetingSystemActive)
        {
            triggerTargetingActive = false;
            crosshair.transform.parent = hmdCrosshairParent.transform;
            // Setze die Position des Fadenkreuzes auf die initiale Position zurück
            crosshair.transform.localPosition = initialCrosshairPosition;
            lastCrosshairPosition = initialCrosshairPosition; // Aktualisiere Referenz für nächsten Trigger-Druck
        }
    }

    private void OnPrimaryRightPressed(InputAction.CallbackContext context)
    {
        targetingSystemActive = !targetingSystemActive;

        if (targetingSystemActive)
        {
            crosshair.SetActive(true);
            crosshair.transform.parent = hmdCrosshairParent.transform; // Fadenkreuz folgt HMD
            triggerTargetingActive = false; // Stelle sicher, dass Trigger-Steuerung deaktiviert ist
            crosshair.transform.localPosition = initialCrosshairPosition; // Setze auf initiale Position
            lastCrosshairPosition = initialCrosshairPosition; // Aktualisiere Referenz
        }
        else
        {
            crosshair.SetActive(false);
            triggerTargetingActive = false; // Deaktiviere Trigger-Steuerung
        }
    }

    // Normalisiert Euler-Winkel von 0-360 zu -180 bis 180
    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}