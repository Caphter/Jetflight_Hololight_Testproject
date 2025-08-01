using UnityEngine;
using UnityEngine.InputSystem;

public class TargetingSystem : MonoBehaviour
{
    [Header("UI Objects:")]
    [SerializeField] private GameObject crosshairWorldAnchor;
    [SerializeField] private GameObject hmdCrosshairParent;
    [SerializeField] private GameObject normalCrosshair;
    [SerializeField] private GameObject lockedCrosshair;

    [Header("Controller & Input:")]
    [SerializeField] private GameObject leftController;
    [SerializeField] private InputActionReference triggerLeftPressedButton;
    [SerializeField] private InputActionReference primaryRightPressedButton;

    [Header("Crosshair Movement:")]
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float maxVerticalAngle = 45f;
    [SerializeField] private float maxHorizontalAngle = 45f;
    [SerializeField] private float verticalOffsetBias = -10f;

    [Header("Target Lock:")]
    [SerializeField] private float targetLockDuration = 3f;
    [SerializeField] private float crosshairDistance = 50f;

    public bool isTargetLocked = false;
    private Transform lockedTargetTransform;
    private float targetLockTimer = 0f;

    public bool triggerTargetingActive = false;
    public bool targetingSystemActive = false;

    private Vector3 initialCrosshairLocalPositionHMD;
    private Vector3 initialCrosshairLocalPositionWorldSpace;
    private Quaternion initialControllerRotationAtPress;

    private Transform mainCameraTransform;

    private void Awake()
    {
        if (hmdCrosshairParent == null || normalCrosshair == null || lockedCrosshair == null || leftController == null || crosshairWorldAnchor == null)
        {
            Debug.LogError("TargetingSystem: Eines der Haupt-GameObjects ist nicht zugewiesen.");
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
            Debug.LogError("TargetingSystem: Keine Kamera mit 'MainCamera' Tag gefunden.");
        }

        triggerLeftPressedButton.action.Enable();
        triggerLeftPressedButton.action.performed += OnTriggerLeftPressed;
        triggerLeftPressedButton.action.canceled += OnTriggerLeftReleased;

        primaryRightPressedButton.action.Enable();
        primaryRightPressedButton.action.performed += OnPrimaryRightPressed;

        // Beim Start sind beide Fadenkreuze unsichtbar
        normalCrosshair.SetActive(false);
        lockedCrosshair.SetActive(false);
        normalCrosshair.transform.SetParent(hmdCrosshairParent.transform, false);
        initialCrosshairLocalPositionHMD = normalCrosshair.transform.localPosition;
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
        // KORRIGIERTER BEREICH: Zentrale Logik zum Ein-/Ausschalten
        if (!targetingSystemActive)
        {
            normalCrosshair.SetActive(false);
            lockedCrosshair.SetActive(false);
            return;
        }

        // Der Rest der Logik wird nur ausgeführt, wenn das System aktiv ist
        if (isTargetLocked)
        {
            normalCrosshair.SetActive(false);
            lockedCrosshair.SetActive(true);
            HandleTargetLock();
        }
        else if (triggerTargetingActive)
        {
            normalCrosshair.SetActive(true);
            lockedCrosshair.SetActive(false);
            HandleControllerBasedTargeting();
        }
        else
        {
            normalCrosshair.SetActive(true);
            lockedCrosshair.SetActive(false);
            normalCrosshair.transform.localPosition = initialCrosshairLocalPositionHMD;
        }

        if (mainCameraTransform != null)
        {
            normalCrosshair.transform.LookAt(mainCameraTransform.position);
            normalCrosshair.transform.Rotate(0, 180, 0, Space.Self);
            lockedCrosshair.transform.LookAt(mainCameraTransform.position);
            lockedCrosshair.transform.Rotate(0, 180, 0, Space.Self);
        }
    }

    private void HandleControllerBasedTargeting()
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

            newLocalPosition.y = Mathf.Clamp(newLocalPosition.y,
                                             initialCrosshairLocalPositionWorldSpace.y - maxVerticalAngle + verticalOffsetBias,
                                             initialCrosshairLocalPositionWorldSpace.y + maxVerticalAngle + verticalOffsetBias);
            newLocalPosition.x = Mathf.Clamp(newLocalPosition.x,
                                             initialCrosshairLocalPositionWorldSpace.x - maxHorizontalAngle,
                                             initialCrosshairLocalPositionWorldSpace.x + maxHorizontalAngle);

            normalCrosshair.transform.localPosition = newLocalPosition;
        }
    }

    private void HandleTargetLock()
    {
        if (lockedTargetTransform != null)
        {
            Vector3 directionToTarget = (lockedTargetTransform.position - mainCameraTransform.position).normalized;
            lockedCrosshair.transform.position = mainCameraTransform.position + directionToTarget * crosshairDistance;

            targetLockTimer -= Time.deltaTime;

            if (targetLockTimer <= 0)
            {
                ReleaseTargetLock();
            }
        }
        else
        {
            ReleaseTargetLock();
        }
    }

    public void SetTargetLock(Transform targetTransform)
    {
        if (targetTransform != null)
        {
            FindObjectOfType<AudioManager>()?.Play("Target_Locked");

            isTargetLocked = true;
            lockedTargetTransform = targetTransform;
            targetLockTimer = targetLockDuration;

            lockedCrosshair.transform.SetParent(null);
        }
    }

    public void ReleaseTargetLock()
    {
        isTargetLocked = false;
        lockedTargetTransform = null;
        targetLockTimer = 0;

        normalCrosshair.SetActive(true);
        lockedCrosshair.SetActive(false);

        lockedCrosshair.transform.SetParent(hmdCrosshairParent.transform, false);
    }

    private void OnTriggerLeftPressed(InputAction.CallbackContext context)
    {
        if (targetingSystemActive)
        {
            triggerTargetingActive = true;

            if (leftController != null)
            {
                initialControllerRotationAtPress = leftController.transform.rotation;

                if (!isTargetLocked)
                {
                    normalCrosshair.transform.SetParent(crosshairWorldAnchor.transform, worldPositionStays: true);
                    initialCrosshairLocalPositionWorldSpace = normalCrosshair.transform.localPosition;
                }
            }
        }
    }

    private void OnTriggerLeftReleased(InputAction.CallbackContext context)
    {
        if (targetingSystemActive)
        {
            triggerTargetingActive = false;

            if (!isTargetLocked && hmdCrosshairParent != null)
            {
                normalCrosshair.transform.SetParent(hmdCrosshairParent.transform, worldPositionStays: true);
            }
        }
    }

    private void OnPrimaryRightPressed(InputAction.CallbackContext context)
    {
        targetingSystemActive = !targetingSystemActive;
        // Die Logik für das Ein- und Ausschalten der Fadenkreuze wurde in die Update-Methode verschoben,
        // um Konflikte zu vermeiden und den Status konsistent zu halten.
        if (targetingSystemActive)
        {
            ReleaseTargetLock();
        }
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) return angle - 360f;
        if (angle < -180f) return angle + 360f;
        return angle;
    }

    public Vector3 GetLockedTargetPosition()
    {
        if (lockedTargetTransform != null)
        {
            return lockedTargetTransform.position;
        }
        return Vector3.zero;
    }
}