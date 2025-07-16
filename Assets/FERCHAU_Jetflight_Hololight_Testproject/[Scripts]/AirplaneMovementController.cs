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

    // NEUE EINSTELLUNGEN FÜR BESCHLEUNIGUNG/ABBREMSUNG
    [Header("Acceleration/Deceleration Settings")]
    [Range(0.1f, 10f)][SerializeField] private float accelerationTime = 1.0f; // Zeit in Sekunden, um auf volle Beschleunigung zu kommen
    [Range(0.1f, 10f)][SerializeField] private float decelerationTime = 2.0f; // Zeit in Sekunden, um auf volle Abbremsung zu kommen

    private float joystickYaw; // Wert von -1 (links) bis 1 (rechts)

    // Speichern der aktuellen Joystick/Thumbstick Inputs nach Deadzone-Anwendung
    private float currentJoystickPitch;
    private float currentJoystickRoll;
    private float currentJoystickYaw;

    public float currentSpeed; // Tatsächliche, geglättete Geschwindigkeit
    private float targetSpeed; // Die von ThrottleSpeedCalc gewünschte Geschwindigkeit

    private Vector3 initialGlobalGravity; // Speichert den ursprünglichen globalen Schwerkraftwert

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
    }

    private void Start()
    {
        rightThumbstick.action.performed += RightThumbstickMoved;
        rightThumbstick.action.canceled += RightThumbstickReleased;

        if (vignetteLogic == null)
        {
            Debug.LogError("MotionSicknessVignetteLogic ist nicht zugewiesen! Bitte im Inspector zuweisen.");
        }

        // Setze initial targetSpeed und currentSpeed auf den Startwert des Throttle
        targetSpeed = throttleSpeedScript.GetCurrentThrottleToSpeedValue();
        currentSpeed = targetSpeed;
    }

    private void OnDestroy()
    {
        rightThumbstick.action.performed -= RightThumbstickMoved;
        rightThumbstick.action.canceled -= RightThumbstickReleased;

        Physics.gravity = initialGlobalGravity; // Setze die globale Schwerkraft zurück
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
        // 1. Hole die ZIELGESCHWINDIGKEIT vom Throttle
        targetSpeed = throttleSpeedScript.GetCurrentThrottleToSpeedValue();

        // 2. Glätte die aktuelle Geschwindigkeit hin zur Zielgeschwindigkeit
        SmoothSpeed();

        // 3. Wende die Bewegung mit der GEGLÄTTETEN Geschwindigkeit an
        Movement();

        // 4. Passe die globale Schwerkraft an (falls Rigidbody vorhanden)
        AdjustGlobalGravity();
    }

    private void Update()
    {
        UpdateVignette();
    }

    /// <summary>
    /// Glättet die aktuelle Geschwindigkeit in Richtung der Zielgeschwindigkeit
    /// basierend auf Beschleunigungs- oder Abbremszeiten.
    /// </summary>
    private void SmoothSpeed()
    {
        float smoothTime;

        if (targetSpeed > currentSpeed)
        {
            // Beschleunigen
            smoothTime = accelerationTime;
        }
        else if (targetSpeed < currentSpeed)
        {
            // Abbremsen
            smoothTime = decelerationTime;
        }
        else
        {
            // Geschwindigkeit ist gleich, keine Glättung nötig
            return;
        }

        // Verwende Mathf.Lerp, um die Geschwindigkeit über die Zeit zu glätten.
        // Je größer smoothTime, desto länger dauert die Anpassung.
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / smoothTime);

        // Optional: Einen kleinen Schwellenwert hinzufügen, um zu verhindern, dass die Geschwindigkeit 
        // sehr nahe an der Zielgeschwindigkeit ist, aber nie ganz erreicht wird (fließkommaungenauigkeit).
        if (Mathf.Abs(currentSpeed - targetSpeed) < 0.01f)
        {
            currentSpeed = targetSpeed;
        }
    }


    private void Movement()
    {
        // Obwohl wir den Rigidbody für die globale Schwerkraft nutzen, 
        // wird die FORWARD-Bewegung weiterhin direkt über Transform.Translate gesteuert,
        // um die gewünschte nicht-physikalische Beschleunigung zu ermöglichen.
        // Der Rigidbody wird weiterhin von der angepassten Physics.gravity beeinflusst.

        jetObject.transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        // --- Read joystick inputs and apply deadzone ---
        float rawJoystickPitch = joystickInput.transform.localRotation.x;
        float rawJoystickRoll = joystickInput.transform.localRotation.z;

        currentJoystickPitch = Mathf.Abs(rawJoystickPitch) < joystickDeadzone ? 0f : rawJoystickPitch;
        currentJoystickRoll = Mathf.Abs(rawJoystickRoll) < joystickDeadzone ? 0f : rawJoystickRoll;
        currentJoystickYaw = Mathf.Abs(joystickYaw) < thumbstickDeadzone ? 0f : joystickYaw;

        // --- Apply rotations to the jet (still using Transform.Rotate as per original intent) ---
        // Wenn du Rotation auch physikalisch über Rigidbody steuern möchtest,
        // müsstest du hier Rigidbody.MoveRotation verwenden.
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
            // Wenn der Jet am Boden ist, volle Schwerkraft anwenden, um ein Abheben zu verhindern.
            Physics.gravity = new Vector3(initialGlobalGravity.x, -maxGlobalGravity, initialGlobalGravity.z);
        }
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