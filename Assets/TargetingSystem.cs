using UnityEngine;
using UnityEngine.InputSystem;

// Enum für die verschiedenen Targeting-Modi
public enum TargetingMode
{
    NoTargeting,
    Crosshair,
    Locked
}

// Datenstruktur für die Rückgabe
public struct TargetingData
{
    public TargetingMode mode;
    public Vector3 targetPosition;
}

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

    // NEU: Referenzen für die Kollisionspunkte zurück in dieses Skript
    [Header("Target Point Sources:")]
    [SerializeField] private CrosshairTargetColliderCheck crosshairTargetColliderCheck;
    [SerializeField] private NoTargetingCollisionCheck noTargetingCollisionCheck;

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
        mainCameraTransform = Camera.main.transform;

        triggerLeftPressedButton.action.Enable();
        triggerLeftPressedButton.action.performed += OnTriggerLeftPressed;
        triggerLeftPressedButton.action.canceled += OnTriggerLeftReleased;

        primaryRightPressedButton.action.Enable();
        primaryRightPressedButton.action.performed += OnPrimaryRightPressed;

        normalCrosshair.SetActive(false);
        lockedCrosshair.SetActive(false);
        normalCrosshair.transform.SetParent(hmdCrosshairParent.transform, false);
        initialCrosshairLocalPositionHMD = normalCrosshair.transform.localPosition;
    }

    private void OnDestroy()
    {
        triggerLeftPressedButton.action.performed -= OnTriggerLeftPressed;
        triggerLeftPressedButton.action.canceled -= OnTriggerLeftReleased;
        primaryRightPressedButton.action.performed -= OnPrimaryRightPressed;
    }

    private void Update()
    {
        if (!targetingSystemActive)
        {
            normalCrosshair.SetActive(false);
            lockedCrosshair.SetActive(false);
            return;
        }

        normalCrosshair.SetActive(!isTargetLocked);
        lockedCrosshair.SetActive(isTargetLocked);

        if (isTargetLocked)
        {
            HandleTargetLock();
        }
        else if (triggerTargetingActive)
        {
            HandleControllerBasedTargeting();
        }
        else
        {
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
        Quaternion deltaControllerRotation = leftController.transform.rotation * Quaternion.Inverse(initialControllerRotationAtPress);
        Quaternion transformedDeltaRotation = Quaternion.Inverse(mainCameraTransform.rotation) * deltaControllerRotation * mainCameraTransform.rotation;
        Vector3 eulerDelta = transformedDeltaRotation.eulerAngles;

        float deltaPitch = -NormalizeAngle(eulerDelta.x) * sensitivity;
        float deltaYaw = NormalizeAngle(eulerDelta.y) * sensitivity;

        Vector3 newLocalPosition = initialCrosshairLocalPositionWorldSpace;
        newLocalPosition.y = Mathf.Clamp(newLocalPosition.y + deltaPitch, initialCrosshairLocalPositionWorldSpace.y - maxVerticalAngle + verticalOffsetBias, initialCrosshairLocalPositionWorldSpace.y + maxVerticalAngle + verticalOffsetBias);
        newLocalPosition.x = Mathf.Clamp(newLocalPosition.x + deltaYaw, initialCrosshairLocalPositionWorldSpace.x - maxHorizontalAngle, initialCrosshairLocalPositionWorldSpace.x + maxHorizontalAngle);

        normalCrosshair.transform.localPosition = newLocalPosition;
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
        lockedCrosshair.SetActive(false);
        lockedCrosshair.transform.SetParent(hmdCrosshairParent.transform, false);
    }

    private void OnTriggerLeftPressed(InputAction.CallbackContext context)
    {
        if (targetingSystemActive)
        {
            triggerTargetingActive = true;
            initialControllerRotationAtPress = leftController.transform.rotation;
            if (!isTargetLocked)
            {
                normalCrosshair.transform.SetParent(crosshairWorldAnchor.transform, worldPositionStays: true);
                initialCrosshairLocalPositionWorldSpace = normalCrosshair.transform.localPosition;
            }
        }
    }

    private void OnTriggerLeftReleased(InputAction.CallbackContext context)
    {
        if (targetingSystemActive)
        {
            triggerTargetingActive = false;
            if (!isTargetLocked)
            {
                normalCrosshair.transform.SetParent(hmdCrosshairParent.transform, worldPositionStays: true);
            }
        }
    }

    private void OnPrimaryRightPressed(InputAction.CallbackContext context)
    {
        targetingSystemActive = !targetingSystemActive;
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
        return lockedTargetTransform != null ? lockedTargetTransform.position : Vector3.zero;
    }

    // NEU: Die öffentliche Funktion, die Targeting-Informationen zurückgibt
    public TargetingData GetTargetingData()
    {
        if (targetingSystemActive)
        {
            if (isTargetLocked)
            {
                return new TargetingData { mode = TargetingMode.Locked, targetPosition = GetLockedTargetPosition() };
            }
            else
            {
                return new TargetingData { mode = TargetingMode.Crosshair, targetPosition = crosshairTargetColliderCheck.currentCollidingPoint };
            }
        }
        else
        {
            return new TargetingData { mode = TargetingMode.NoTargeting, targetPosition = noTargetingCollisionCheck.currentCollidingPoint };
        }
    }
}