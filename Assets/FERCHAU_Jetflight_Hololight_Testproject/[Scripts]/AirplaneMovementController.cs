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

    [Header("Rotating Speeds")]
    [Range(5f, 500f)][SerializeField] private float pitchSpeed = 100f; // Pitch speed
    [Range(5f, 500f)][SerializeField] private float rollSpeed = 200f; // Roll speed
    [Range(5f, 200f)][SerializeField] private float yawSpeed = 50f; // Yaw speed (rotation around Y-axis)

    [Header("Deadzone Settings")]
    [Range(0f, 1f)][SerializeField] private float joystickDeadzone = 0.05f; // Deadzone for joystick deflection (0.0 - 1.0)
    [Range(0f, 1f)][SerializeField] private float thumbstickDeadzone = 0.1f; // Deadzone for thumbstick deflection (0.0 - 1.0)

    public InputActionReference rightThumbstick;
    private float joystickYaw;

    public float currentSpeed; // Calculated speed

    private void Start()
    {
        // Subscribe to the performed and canceled events of the thumbstick
        rightThumbstick.action.performed += RightThumbstickMoved;
        rightThumbstick.action.canceled += RightThumbstickReleased;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
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
        // Reset joystickYaw to 0 when the thumbstick is released
        joystickYaw = 0f;
    }

    private void Update()
    {
        // Get speed from the throttle script
        currentSpeed = throttleSpeedScript.GetCurrentThrottleToSpeedValue();

        // Move and rotate the airplane
        Movement();
    }

    private void Movement()
    {
        // Forward movement based on current speed
        jetObject.transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        // --- Read joystick inputs and apply deadzone ---
        float joystickPitch = joystickInput.transform.localRotation.x; // Pitch angle from joystick
        float joystickRoll = joystickInput.transform.localRotation.z; // Roll angle from joystick

        // Apply deadzone for pitch
        if (Mathf.Abs(joystickPitch) < joystickDeadzone)
        {
            joystickPitch = 0f;
        }

        // Apply deadzone for roll
        if (Mathf.Abs(joystickRoll) < joystickDeadzone)
        {
            joystickRoll = 0f;
        }

        // --- Apply deadzone for yaw (thumbstick) ---
        if (Mathf.Abs(joystickYaw) < thumbstickDeadzone)
        {
            joystickYaw = 0f;
        }

        // --- Apply rotations to the jet ---
        // Apply roll rotation
        jetObject.transform.Rotate(Vector3.forward * joystickRoll * rollSpeed * Time.deltaTime);

        // Apply pitch rotation
        jetObject.transform.Rotate(Vector3.right * joystickPitch * pitchSpeed * Time.deltaTime);

        // Apply yaw rotation (based on thumbstick)
        jetObject.transform.Rotate(Vector3.up * joystickYaw * yawSpeed * Time.deltaTime);
    }
}