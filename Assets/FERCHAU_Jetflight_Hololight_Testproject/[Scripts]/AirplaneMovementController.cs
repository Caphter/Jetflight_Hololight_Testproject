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

    // Einstellungen für die dynamische globale Schwerkraft
    [Header("Dynamic Global Gravity Settings")]
    [SerializeField] private float maxGlobalGravity = 9.81f; // Maximale globale Schwerkraft (positiver Wert)
    [SerializeField] private float minSpeedForZeroGlobalGravity = 100f; // Geschwindigkeit, bei der die globale Schwerkraft 0 wird
    // Referenz zum Rigidbody des Jets, um sicherzustellen, dass er die Physik nutzt
    [SerializeField] private Rigidbody jetRigidbody;

    public GroundContactManager groundContactManagerScript;

    // EINSTELLUNGEN FÜR BESCHLEUNIGUNG/ABBREMSUNG
    [Header("Acceleration/Deceleration Settings")]
    [Range(0.1f, 10f)][SerializeField] private float accelerationTime = 1.0f; // Zeit in Sekunden, um auf volle Beschleunigung zu kommen
    [Range(0.1f, 10f)][SerializeField] private float decelerationTime = 2.0f; // Zeit in Sekunden, um auf volle Abbremsung zu kommen

    // NEUE SOUND-EINSTELLUNGEN
    [Header("Engine Sound Settings")]
    [SerializeField] private AudioSource engineAudioSource; // AudioSource für den Triebwerkssound
    [Range(0.1f, 3.0f)][SerializeField] private float minPitch = 0.5f; // Minimale Tonhöhe des Sounds (bei 0 Geschwindigkeit)
    [Range(0.1f, 3.0f)][SerializeField] private float maxPitch = 2.0f; // Maximale Tonhöhe des Sounds (bei MaxSpeed)
    [Range(0f, 1.0f)][SerializeField] private float minVolume = 0.2f; // Minimale Lautstärke (bei 0 Geschwindigkeit)
    [Range(0f, 1.0f)][SerializeField] private float maxVolume = 1.0f; // Maximale Lautstärke (bei MaxSpeed)
    [Range(0.1f, 5f)][SerializeField] private float soundSmoothSpeed = 1.0f; // Glättungsfaktor für Sound-Änderungen

    private float joystickYaw; // Wert von -1 (links) bis 1 (rechts)

    // Speichern der aktuellen Joystick/Thumbstick Inputs nach Deadzone-Anwendung
    private float currentJoystickPitch;
    private float currentJoystickRoll;
    private float currentJoystickYaw;

    public float currentSpeed; // Tatsächliche, geglättete Geschwindigkeit
    private float targetSpeed; // Die von ThrottleSpeedCalc gewünschte Geschwindigkeit

    private Vector3 initialGlobalGravity; // Speichert den ursprünglichen globalen Schwerkraftwert

    private float throttleMaxSpeed; // Holt den maximalen Geschwindigkeitswert vom ThrottleScript

    public bool collisionFreeze = false;

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

        // Sicherstellen, dass eine AudioSource zugewiesen oder gefunden wird
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

        if (vignetteLogic == null)
        {
            Debug.LogError("MotionSicknessVignetteLogic ist nicht zugewiesen! Bitte im Inspector zuweisen.");
        }

        // Holen des maximalen Geschwindigkeitswerts vom ThrottleSpeedCalc
        if (throttleSpeedScript != null)
        {
            throttleMaxSpeed = throttleSpeedScript.GetMaxThrottleToSpeedValue(); // Angenommen, diese Methode existiert in ThrottleSpeedCalc
        }
        else
        {
            Debug.LogError("ThrottleSpeedCalc ist nicht zugewiesen! MaxSpeed kann nicht ermittelt werden.");
            throttleMaxSpeed = 100f; // Fallback-Wert
        }

        // Setze initial targetSpeed und currentSpeed auf den Startwert des Throttle
        targetSpeed = throttleSpeedScript.GetCurrentThrottleToSpeedValue();
        currentSpeed = targetSpeed;

        // Sound abspielen, wenn er noch nicht läuft
        if (engineAudioSource != null && !engineAudioSource.isPlaying)
        {
            engineAudioSource.loop = true; // Stellen Sie sicher, dass der Sound loopt
            engineAudioSource.Play();
        }
    }

    private void OnDestroy()
    {
        rightThumbstick.action.performed -= RightThumbstickMoved;
        rightThumbstick.action.canceled -= RightThumbstickReleased;

        Physics.gravity = initialGlobalGravity; // Setze die globale Schwerkraft zurück

        if (engineAudioSource != null)
        {
            engineAudioSource.Stop(); // Stoppe den Sound beim Zerstören des Skripts
        }
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

    private void FixedUpdate()
    {
        if(collisionFreeze)
        {
            return;
        }

        targetSpeed = throttleSpeedScript.GetCurrentThrottleToSpeedValue();
        SmoothSpeed();
        Movement();
        AdjustGlobalGravity();
    }

    private void Update()
    {
        if (collisionFreeze)
        {
            return;
        }

        UpdateEngineSound();
        UpdateVignette();
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

        float rawJoystickPitch = joystickInput.transform.localRotation.x;
        float rawJoystickRoll = joystickInput.transform.localRotation.z;

        currentJoystickPitch = Mathf.Abs(rawJoystickPitch) < joystickDeadzone ? 0f : rawJoystickPitch;
        currentJoystickRoll = Mathf.Abs(rawJoystickRoll) < joystickDeadzone ? 0f : rawJoystickRoll;
        currentJoystickYaw = Mathf.Abs(joystickYaw) < thumbstickDeadzone ? 0f : joystickYaw;

        jetObject.transform.Rotate(Vector3.forward * currentJoystickRoll * rollSpeed * Time.deltaTime);
        jetObject.transform.Rotate(Vector3.right * currentJoystickPitch * pitchSpeed * Time.deltaTime);
        jetObject.transform.Rotate(Vector3.up * currentJoystickYaw * yawSpeed * Time.deltaTime);
    }

    private void AdjustGlobalGravity()
    {
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

    /// <summary>
    /// Aktualisiert die Tonhöhe und Lautstärke des Triebwerkssounds basierend auf der aktuellen Geschwindigkeit.
    /// </summary>
    private void UpdateEngineSound()
    {
        if (engineAudioSource == null || throttleMaxSpeed <= 0) return;

        // Normalisiere die aktuelle Geschwindigkeit im Bereich von 0 bis 1
        // clamp01 stellt sicher, dass der Wert zwischen 0 und 1 bleibt, auch wenn currentSpeed mal über throttleMaxSpeed geht
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / throttleMaxSpeed);

        // Interpoliere die Tonhöhe basierend auf der normalisierten Geschwindigkeit
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, normalizedSpeed);
        // Interpoliere die Lautstärke basierend auf der normalisierten Geschwindigkeit
        float targetVolume = Mathf.Lerp(minVolume, maxVolume, normalizedSpeed);

        // Glätte die Änderungen an Pitch und Volume für einen natürlicheren Übergang
        engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, targetPitch, Time.deltaTime * soundSmoothSpeed);
        engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, targetVolume, Time.deltaTime * soundSmoothSpeed);
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