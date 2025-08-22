using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class AirplaneMovementController : MonoBehaviour
{
    [Header("Joystick Input")]
    [SerializeField] private JoystickConstraintLogic joystickInput;
    [SerializeField] private ThrottleSpeedCalc throttleSpeedScript;
    [SerializeField] private GameObject jetObject;

    [Header("VR Haptic Player")]
    [SerializeField] private HapticImpulsePlayer rightHapticPlayer;

    [Header("Rotating Speeds")]
    [Range(5f, 500f)][SerializeField] private float pitchSpeed = 100f;
    [Range(5f, 500f)][SerializeField] private float rollSpeed = 200f;
    [Range(5f, 200f)][SerializeField] private float yawSpeed = 50f;

    [Header("Deadzone Settings")]
    [Range(0f, 1f)][SerializeField] private float joystickDeadzone = 0.05f;
    [Range(0f, 1f)][SerializeField] private float thumbstickDeadzone = 0.1f;

    [Header("Input Response Curves")]
    [SerializeField] private AnimationCurve pitchCurve;
    [SerializeField] private AnimationCurve rollCurve;

    [Header("Haptics")]
    [Range(0f, 1f)][SerializeField] private float hapticIntensityMultiplier = 0.5f;
    [Range(0f, 1f)][SerializeField] private float hapticDuration = 0.1f;
    [SerializeField] private float minHapticInputThreshold = 0.01f;
    [Tooltip("Stärke der Vibration nur für die Gier-Achse (Yaw)")]
    [Range(0f, 1f)][SerializeField] private float yawHapticMultiplier = 0.25f;

    [Header("Auto-Stabilization Settings")]
    [Tooltip("Geschwindigkeit, mit der das Flugzeug auf Pitch (X-Achse) und Roll (Z-Achse) stabilisiert wird, wenn der Joystick in der Deadzone ist.")]
    [Range(0.1f, 10f)][SerializeField] private float stabilizationSpeed = 2.0f;

    [Tooltip("Maximale Gier-Neigung (in Grad), bei der die automatische Stabilisierung noch versucht, auf Null zu stellen. Bei größeren Neigungen hat der Spieler volle Kontrolle.")]
    [Range(0f, 90f)][SerializeField] private float maxYawStabilizationAngle = 45f;

    [Tooltip("Der Winkel-Schwellenwert (in Grad), unter dem die automatische Stabilisierung den Wert als 'erreicht' betrachtet und weitere Korrekturen stoppt.")]
    [Range(0.001f, 1f)][SerializeField] private float stabilizationAngleThreshold = 0.05f;

    public InputActionReference rightThumbstick;

    [Header("Dynamic Global Gravity Settings")]
    [SerializeField] private float maxGlobalGravity = 9.81f;
    [SerializeField] private float minSpeedForZeroGlobalGravity = 100f;
    [SerializeField] private Rigidbody jetRigidbody;
    [SerializeField] private EjectionSeatLogic ejectionSeatLogicScript;

    public GroundContactManager groundContactManagerScript;

    [Header("Acceleration/Deceleration Settings")]
    [Range(0.1f, 10f)][SerializeField] private float accelerationTime = 1.0f;
    [Range(0.1f, 10f)][SerializeField] private float decelerationTime = 2.0f;

    [Header("Engine Sound Settings")]
    [SerializeField] private AudioSource engineAudioSource;
    [Range(0.1f, 3.0f)][SerializeField] private float minPitch = 0.5f;
    [Range(0.1f, 3.0f)][SerializeField] private float maxPitch = 2.0f;
    [Range(0f, 1.0f)][SerializeField] private float minVolume = 0.2f;
    [Range(0f, 1.0f)][SerializeField] private float maxVolume = 1.0f;
    [Range(0.1f, 5f)][SerializeField] private float soundSmoothSpeed = 1.0f;

    private float joystickYaw;

    public float CurrentJoystickPitch { get; private set; }
    public float CurrentJoystickRoll { get; private set; }
    public float CurrentJoystickYaw { get; private set; }

    public float currentSpeed;
    private float targetSpeed;

    private Vector3 initialGlobalGravity;

    private float throttleMaxSpeed;

    public bool collisionFreeze = false;

    public bool controllerIsHeld = false;

    private void Awake()
    {
        initialGlobalGravity = Physics.gravity;

        if (jetRigidbody == null)
        {
            jetRigidbody = jetObject.GetComponent<Rigidbody>();
            if (jetRigidbody == null)
            {
                Debug.LogError("JetObject hat keinen Rigidbody zugewiesen oder gefunden! Globale Schwerkraftanpassung könnte Probleme verursachen.");
            }
        }

        if (engineAudioSource == null)
        {
            engineAudioSource = jetObject.GetComponent<AudioSource>();
            if (engineAudioSource == null)
            {
                Debug.LogError("JetObject hat keine AudioSource zugewiesen oder gefunden! Triebwerkssound wird nicht abgespielt.");
            }
        }
    }

    private void Start()
    {
        rightThumbstick.action.performed += RightThumbstickMoved;
        rightThumbstick.action.canceled += RightThumbstickReleased;

        if (throttleSpeedScript != null)
        {
            throttleMaxSpeed = throttleSpeedScript.GetMaxThrottleToSpeedValue();
        }
        else
        {
            Debug.LogError("ThrottleSpeedCalc ist nicht zugewiesen! MaxSpeed kann nicht ermittelt werden.");
            throttleMaxSpeed = 100f;
        }

        targetSpeed = throttleSpeedScript.GetCurrentThrottleToSpeedValue();
        currentSpeed = targetSpeed;

        if (engineAudioSource != null && !engineAudioSource.isPlaying)
        {
            engineAudioSource.loop = true;
            engineAudioSource.Play();
        }
    }

    private void OnDestroy()
    {
        rightThumbstick.action.performed -= RightThumbstickMoved;
        rightThumbstick.action.canceled -= RightThumbstickReleased;

        Physics.gravity = initialGlobalGravity;

        if (engineAudioSource != null)
        {
            engineAudioSource.Stop();
        }
    }

    private void RightThumbstickMoved(InputAction.CallbackContext context)
    {
        Vector2 extractedVector = context.ReadValue<Vector2>();
        joystickYaw = extractedVector.x;
    }

    private void RightThumbstickReleased(InputAction.CallbackContext context)
    {
        joystickYaw = 0f;
    }

    private void FixedUpdate()
    {
        if (collisionFreeze)
        {
            return;
        }

        targetSpeed = throttleSpeedScript.GetCurrentThrottleToSpeedValue();
        SmoothSpeed();
        Movement();
        AdjustGlobalGravity();
        UpdateHaptics();
    }

    private void Update()
    {
        if (collisionFreeze)
        {
            return;
        }

        UpdateEngineSound();
    }

    private void UpdateHaptics()
    {
        if (rightHapticPlayer == null) return;

        float yawMagnitude = Mathf.Abs(CurrentJoystickYaw) * yawHapticMultiplier;

        float combinedInputMagnitude = new Vector3(CurrentJoystickPitch, CurrentJoystickRoll, yawMagnitude).magnitude;

        if (combinedInputMagnitude > minHapticInputThreshold)
        {
            float hapticIntensity = combinedInputMagnitude * hapticIntensityMultiplier;
            rightHapticPlayer.SendHapticImpulse(hapticIntensity, hapticDuration);
        }
        else
        {
            rightHapticPlayer.SendHapticImpulse(0, 0);
        }
    }

    private void SmoothSpeed()
    {
        float smoothTime;

        if (targetSpeed > currentSpeed)
        {
            smoothTime = accelerationTime;
        }
        else if (targetSpeed < currentSpeed)
        {
            smoothTime = decelerationTime;
        }
        else
        {
            return;
        }

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / smoothTime);

        if (Mathf.Abs(currentSpeed - targetSpeed) < 0.01f)
        {
            currentSpeed = targetSpeed;
        }
    }

    private void Movement()
    {
        jetObject.transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        float rawJoystickPitch = joystickInput.NormalizedPitch;
        float rawJoystickRoll = joystickInput.NormalizedRoll;

        CurrentJoystickPitch = Mathf.Abs(rawJoystickPitch) < joystickDeadzone ? 0f : rawJoystickPitch;
        CurrentJoystickRoll = Mathf.Abs(rawJoystickRoll) < joystickDeadzone ? 0f : rawJoystickRoll;
        CurrentJoystickYaw = Mathf.Abs(joystickYaw) < thumbstickDeadzone ? 0f : joystickYaw;

        float curvedPitch = pitchCurve.Evaluate(Mathf.Abs(CurrentJoystickPitch)) * Mathf.Sign(CurrentJoystickPitch);
        float curvedRoll = rollCurve.Evaluate(Mathf.Abs(CurrentJoystickRoll)) * Mathf.Sign(CurrentJoystickRoll);
        float curvedYaw = CurrentJoystickYaw;

        jetObject.transform.Rotate(Vector3.forward * curvedRoll * rollSpeed * Time.deltaTime);
        jetObject.transform.Rotate(Vector3.right * curvedPitch * pitchSpeed * Time.deltaTime);
        jetObject.transform.Rotate(Vector3.up * curvedYaw * yawSpeed * Time.deltaTime);

        if (!controllerIsHeld && CurrentJoystickPitch == 0f && CurrentJoystickRoll == 0f && CurrentJoystickYaw == 0f)
        {
            AutoStabilize();
        }
    }

    private void AutoStabilize()
    {
        Vector3 currentEuler = jetObject.transform.localEulerAngles;

        float normalizedPitch = NormalizeAngle(currentEuler.x);
        float normalizedRoll = NormalizeAngle(currentEuler.z);
        float normalizedYaw = NormalizeAngle(currentEuler.y);

        if (Mathf.Abs(normalizedPitch) > stabilizationAngleThreshold)
        {
            float pitchCorrection = -normalizedPitch * stabilizationSpeed * Time.deltaTime;
            jetObject.transform.Rotate(Vector3.right, pitchCorrection, Space.Self);
        }
        else if (Mathf.Abs(normalizedPitch) <= stabilizationAngleThreshold && normalizedPitch != 0f)
        {
            jetObject.transform.localRotation = Quaternion.Euler(0, jetObject.transform.localEulerAngles.y, jetObject.transform.localEulerAngles.z);
        }

        if (Mathf.Abs(normalizedRoll) > stabilizationAngleThreshold)
        {
            float rollCorrection = -normalizedRoll * stabilizationSpeed * Time.deltaTime;
            jetObject.transform.Rotate(Vector3.forward, rollCorrection, Space.Self);
        }
        else if (Mathf.Abs(normalizedRoll) <= stabilizationAngleThreshold && normalizedRoll != 0f)
        {
            jetObject.transform.localRotation = Quaternion.Euler(jetObject.transform.localEulerAngles.x, jetObject.transform.localEulerAngles.y, 0);
        }

        if (Mathf.Abs(normalizedYaw) > stabilizationAngleThreshold && Mathf.Abs(normalizedYaw) <= maxYawStabilizationAngle)
        {
            float yawCorrection = -normalizedYaw * stabilizationSpeed * Time.deltaTime * 0.5f;
            jetObject.transform.Rotate(Vector3.up, yawCorrection, Space.Self);
        }
        else if (Mathf.Abs(normalizedYaw) <= stabilizationAngleThreshold && normalizedYaw != 0f && CurrentJoystickYaw == 0f)
        {
            jetObject.transform.localRotation = Quaternion.Euler(jetObject.transform.localEulerAngles.x, 0, jetObject.transform.localEulerAngles.z);
        }
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    private void AdjustGlobalGravity()
    {
        if (ejectionSeatLogicScript.ejectionSequenceStarted)
        {
            Physics.gravity = initialGlobalGravity;
            return;
        }
        if (!groundContactManagerScript.isGrounded)
        {
            float gravityMultiplier = Mathf.InverseLerp(minSpeedForZeroGlobalGravity, 0, currentSpeed);
            Physics.gravity = new Vector3(initialGlobalGravity.x, -maxGlobalGravity * gravityMultiplier, initialGlobalGravity.z);
        }
        else
        {
            Physics.gravity = new Vector3(initialGlobalGravity.x, -maxGlobalGravity, initialGlobalGravity.z);
        }
    }

    private void UpdateEngineSound()
    {
        if (engineAudioSource == null || throttleMaxSpeed <= 0) return;

        float normalizedSpeed = Mathf.Clamp01(currentSpeed / throttleMaxSpeed);

        float targetPitch = Mathf.Lerp(minPitch, maxPitch, normalizedSpeed);
        float targetVolume = Mathf.Lerp(minVolume, maxVolume, normalizedSpeed);

        engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, targetPitch, Time.deltaTime * soundSmoothSpeed);
        engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, targetVolume, Time.deltaTime * soundSmoothSpeed);
    }

    public float PitchSpeed => pitchSpeed;
    public float RollSpeed => rollSpeed;
    public float YawSpeed => yawSpeed;
}