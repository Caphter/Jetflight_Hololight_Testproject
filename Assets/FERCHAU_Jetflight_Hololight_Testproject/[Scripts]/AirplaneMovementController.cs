using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the movement of the airplane based on joystick input. It calculates the speed of the airplane and moves it forward.
/// The airplane can be rotated around the x, y, and z axes (Pitch, Roll, and Yaw).
/// </summary>
public class AirplaneMovementController : MonoBehaviour
{
    [Header("Joystick Input")]
    [SerializeField] private GameObject joystickInput; // Joystick for control
    [SerializeField] private ThrottleSpeedCalc throttleSpeedScript; // Speed calculation script
    [SerializeField] private GameObject jetObject;
    [SerializeField] private MotionSicknessVignetteLogic vignetteLogic; // Referenz zum Vignette-Skript

    [Header("Rotating Speeds")]
    [Range(5f, 500f)][SerializeField] private float pitchSpeed = 100f; // Pitch speed
    [Range(5f, 500f)][SerializeField] private float rollSpeed = 200f; // Roll speed
    [Range(5f, 200f)][SerializeField] private float yawSpeed = 50f; // Yaw speed (rotation around Y-axis)

    [Header("Deadzone Settings")]
    [Range(0f, 1f)][SerializeField] private float joystickDeadzone = 0.05f; // Deadzone for joystick deflection
    [Range(0f, 1f)][SerializeField] private float thumbstickDeadzone = 0.1f; // Deadzone for thumbstick deflection

    public InputActionReference rightThumbstick;
    private float joystickYaw; // Wert von -1 (links) bis 1 (rechts)

    // Speichern der aktuellen Joystick/Thumbstick Inputs nach Deadzone-Anwendung
    private float currentJoystickPitch;
    private float currentJoystickRoll;
    private float currentJoystickYaw;

    public float currentSpeed; // Calculated speed

    private void Start()
    {
        rightThumbstick.action.performed += RightThumbstickMoved;
        rightThumbstick.action.canceled += RightThumbstickReleased;

        if (vignetteLogic == null)
        {
            Debug.LogError("MotionSicknessVignetteLogic ist nicht zugewiesen! Bitte im Inspector zuweisen.");
        }
    }

    private void OnDestroy()
    {
        rightThumbstick.action.performed -= RightThumbstickMoved;
        rightThumbstick.action.canceled -= RightThumbstickReleased;
    }

    void RightThumbstickMoved(InputAction.CallbackContext context)
    {
        Vector2 extractedVector = context.ReadValue<Vector2>();
        joystickYaw = extractedVector.x;
    }

    void RightThumbstickReleased(InputAction.CallbackContext context)
    {
        joystickYaw = 0f;
    }

    private void Update()
    {
        currentSpeed = throttleSpeedScript.GetCurrentThrottleToSpeedValue();
        Movement();
        UpdateVignette();
    }

    private void Movement()
    {
        jetObject.transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        // --- Read joystick inputs and apply deadzone ---
        float rawJoystickPitch = joystickInput.transform.localRotation.x;
        float rawJoystickRoll = joystickInput.transform.localRotation.z;

        currentJoystickPitch = Mathf.Abs(rawJoystickPitch) < joystickDeadzone ? 0f : rawJoystickPitch;
        currentJoystickRoll = Mathf.Abs(rawJoystickRoll) < joystickDeadzone ? 0f : rawJoystickRoll;
        currentJoystickYaw = Mathf.Abs(joystickYaw) < thumbstickDeadzone ? 0f : joystickYaw;

        // --- Apply rotations to the jet ---
        jetObject.transform.Rotate(Vector3.forward * currentJoystickRoll * rollSpeed * Time.deltaTime);
        jetObject.transform.Rotate(Vector3.right * currentJoystickPitch * pitchSpeed * Time.deltaTime);
        jetObject.transform.Rotate(Vector3.up * currentJoystickYaw * yawSpeed * Time.deltaTime);
    }

    private void UpdateVignette()
    {
        if (vignetteLogic != null)
        {
            vignetteLogic.UpdateVignette(currentJoystickPitch, currentJoystickRoll, currentJoystickYaw,
                                        pitchSpeed, rollSpeed, yawSpeed);
        }
    }
}