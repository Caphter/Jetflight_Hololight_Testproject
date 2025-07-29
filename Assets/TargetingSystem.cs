using UnityEngine;
using UnityEngine.InputSystem;

public class TargetingSystem : MonoBehaviour
{
    [SerializeField] private GameObject crosshairWorldAnchor;
    [SerializeField] private GameObject hmdCrosshairParent;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject leftController;

    [SerializeField] private InputActionReference triggerLeftPressedButton;
    [SerializeField] private InputActionReference primaryRightPressedButton;

    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float maxVerticalAngle = 45f;
    [SerializeField] private float maxHorizontalAngle = 45f;
    [SerializeField] private float verticalOffsetBias = -10f; // NEU: Positiver Wert verschiebt Bereich nach unten, negativer nach oben.

    public bool triggerTargetingActive = false;
    public bool targetingSystemActive = false;

    private Vector3 initialCrosshairLocalPositionHMD;
    private Vector3 initialCrosshairLocalPositionWorldSpace;
    private Quaternion initialControllerRotationAtPress;

    private Transform mainCameraTransform;

    private void Awake()
    {
        if (hmdCrosshairParent == null || crosshair == null || leftController == null || crosshairWorldAnchor == null)
        {
            Debug.LogError("TargetingSystem: Eines der Haupt-GameObjects (HMD Parent, World Anchor, Crosshair, Left Controller) ist nicht zugewiesen.");
            enabled = false;
            return;
        }
        if (triggerLeftPressedButton == null || primaryRightPressedButton == null ||
            triggerLeftPressedButton.action == null || primaryRightPressedButton.action == null)
        {
            Debug.LogError("TargetingSystem: Eine oder beide Input Actions sind nicht zugewiesen oder initialisiert.");
            enabled = false;
            return;
        }

        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("TargetingSystem: Keine Kamera mit 'MainCamera' Tag gefunden. Fadenkreuz kann sich nicht ausrichten.");
        }

        triggerLeftPressedButton.action.Enable();
        triggerLeftPressedButton.action.performed += OnTriggerLeftPressed;
        triggerLeftPressedButton.action.canceled += OnTriggerLeftReleased;

        primaryRightPressedButton.action.Enable();
        primaryRightPressedButton.action.performed += OnPrimaryRightPressed;

        crosshair.SetActive(false);
        crosshair.transform.SetParent(hmdCrosshairParent.transform, false);
        initialCrosshairLocalPositionHMD = crosshair.transform.localPosition;
    }

    private void OnDestroy()
    {
        if (triggerLeftPressedButton != null && triggerLeftPressedButton.action != null)
        {
            triggerLeftPressedButton.action.performed -= OnTriggerLeftPressed;
            triggerLeftPressedButton.action.canceled -= OnTriggerLeftReleased;
        }
        if (primaryRightPressedButton != null && primaryRightPressedButton.action != null)
        {
            primaryRightPressedButton.action.performed -= OnPrimaryRightPressed;
        }
    }

    private void Update()
    {
        if (!targetingSystemActive) return;

        crosshair.SetActive(true);

        if (triggerTargetingActive)
        {
            if (leftController != null && mainCameraTransform != null)
            {
                Quaternion deltaControllerRotation = leftController.transform.rotation * Quaternion.Inverse(initialControllerRotationAtPress);

                Quaternion transformedDeltaRotation = Quaternion.Inverse(mainCameraTransform.rotation) * deltaControllerRotation * mainCameraTransform.rotation;
                Vector3 eulerDelta = transformedDeltaRotation.eulerAngles;

                float deltaPitch = -NormalizeAngle(eulerDelta.x) * sensitivity;
                float deltaYaw = NormalizeAngle(eulerDelta.y) * sensitivity;

                Vector3 newLocalPosition = initialCrosshairLocalPositionWorldSpace;
                newLocalPosition.y += deltaPitch;
                newLocalPosition.x += deltaYaw;

                // Anpassung der vertikalen Grenzwerte
                newLocalPosition.y = Mathf.Clamp(newLocalPosition.y,
                                                 initialCrosshairLocalPositionWorldSpace.y - maxVerticalAngle + verticalOffsetBias, // Unterer Grenzwert verschoben
                                                 initialCrosshairLocalPositionWorldSpace.y + maxVerticalAngle + verticalOffsetBias); // Oberer Grenzwert verschoben
                newLocalPosition.x = Mathf.Clamp(newLocalPosition.x,
                                                 initialCrosshairLocalPositionWorldSpace.x - maxHorizontalAngle,
                                                 initialCrosshairLocalPositionWorldSpace.x + maxHorizontalAngle);

                crosshair.transform.localPosition = newLocalPosition;
            }
        }
        else
        {
            crosshair.transform.localPosition = initialCrosshairLocalPositionHMD;
        }

        if (mainCameraTransform != null)
        {
            crosshair.transform.LookAt(mainCameraTransform.position);
            crosshair.transform.Rotate(0, 180, 0, Space.Self);
        }
    }

    private void OnTriggerLeftPressed(InputAction.CallbackContext context)
    {
        if (targetingSystemActive)
        {
            triggerTargetingActive = true;

            if (leftController != null)
            {
                initialControllerRotationAtPress = leftController.transform.rotation;

                crosshair.transform.SetParent(crosshairWorldAnchor.transform, worldPositionStays: true);
                initialCrosshairLocalPositionWorldSpace = crosshair.transform.localPosition;
            }
        }
    }

    private void OnTriggerLeftReleased(InputAction.CallbackContext context)
    {
        if (targetingSystemActive)
        {
            triggerTargetingActive = false;

            if (hmdCrosshairParent != null)
            {
                crosshair.transform.SetParent(hmdCrosshairParent.transform, worldPositionStays: true);
            }
        }
    }

    private void OnPrimaryRightPressed(InputAction.CallbackContext context)
    {
        targetingSystemActive = !targetingSystemActive;
        Debug.Log($"Targeting System Active: {targetingSystemActive}");

        if (targetingSystemActive)
        {
            crosshair.SetActive(true);
            crosshair.transform.SetParent(hmdCrosshairParent.transform, false);
            crosshair.transform.localPosition = initialCrosshairLocalPositionHMD;
            triggerTargetingActive = false;
        }
        else
        {
            crosshair.SetActive(false);
            triggerTargetingActive = false;
            crosshair.transform.SetParent(hmdCrosshairParent.transform, true);
        }
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) return angle - 360f;
        if (angle < -180f) return angle + 360f;
        return angle;
    }
}