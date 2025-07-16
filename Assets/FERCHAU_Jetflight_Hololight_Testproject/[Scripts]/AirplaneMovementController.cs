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

    // Du hast GroundContactManagerScript bereits, behalte es.
    public GroundContactManager groundContactManagerScript;

    private float joystickYaw; // Wert von -1 (links) bis 1 (rechts)

    // Speichern der aktuellen Joystick/Thumbstick Inputs nach Deadzone-Anwendung
    private float currentJoystickPitch;
    private float currentJoystickRoll;
    private float currentJoystickYaw;

    public float currentSpeed; // Calculated speed

    private Vector3 initialGlobalGravity; // Speichert den ursprünglichen globalen Schwerkraftwert

    private void Awake()
    {
        // Speichere den ursprünglichen globalen Schwerkraftwert von Unity
        initialGlobalGravity = Physics.gravity;

        // Sicherstellen, dass ein Rigidbody zugewiesen ist oder gefunden wird
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
    }

    private void OnDestroy()
    {
        rightThumbstick.action.performed -= RightThumbstickMoved;
        rightThumbstick.action.canceled -= RightThumbstickReleased;

        // Setze die globale Schwerkraft auf den ursprünglichen Wert zurück, wenn das Skript zerstört wird
        Physics.gravity = initialGlobalGravity;
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
        // Physik-Updates sollten in FixedUpdate erfolgen
        currentSpeed = throttleSpeedScript.GetCurrentThrottleToSpeedValue();
        Movement();
        AdjustGlobalGravity(); // Globale Schwerkraft anpassen
    }

    private void Update()
    {
        // Nicht-physikalische Updates wie Vignette
        UpdateVignette();
    }

    private void Movement()
    {
        // Wenn du die globale Schwerkraft verwendest, solltest du auch physikalische Methoden für die Bewegung nutzen.
        // Wenn der Jet keinen Rigidbody hat oder du ihn nicht physikalisch bewegen willst,
        // kannst du weiterhin transform.Translate verwenden, aber die Schwerkraft wirkt dann
        // nur auf andere Rigidbodies in der Szene.
        if (jetRigidbody != null)
        {
            // Bewegen des Rigidbodies
            jetRigidbody.MovePosition(jetRigidbody.position + jetObject.transform.forward * currentSpeed * Time.deltaTime);

            // Rotationen anwenden
            Quaternion pitchRotation = Quaternion.AngleAxis(currentJoystickPitch * pitchSpeed * Time.deltaTime, Vector3.right);
            Quaternion rollRotation = Quaternion.AngleAxis(currentJoystickRoll * rollSpeed * Time.deltaTime, Vector3.forward);
            Quaternion yawRotation = Quaternion.AngleAxis(currentJoystickYaw * yawSpeed * Time.deltaTime, Vector3.up);

            jetRigidbody.MoveRotation(jetRigidbody.rotation * yawRotation * pitchRotation * rollRotation);
        }
        else
        {
            // Fallback, wenn kein Rigidbody zugewiesen ist, behalte die Transform-Bewegung bei.
            // Beachte, dass die globale Schwerkraft in diesem Fall nicht direkt auf den Jet wirkt.
            jetObject.transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
            jetObject.transform.Rotate(Vector3.forward * currentJoystickRoll * rollSpeed * Time.deltaTime);
            jetObject.transform.Rotate(Vector3.right * currentJoystickPitch * pitchSpeed * Time.deltaTime);
            jetObject.transform.Rotate(Vector3.up * currentJoystickYaw * yawSpeed * Time.deltaTime);
        }

        // --- Read joystick inputs and apply deadzone ---
        float rawJoystickPitch = joystickInput.transform.localRotation.x;
        float rawJoystickRoll = joystickInput.transform.localRotation.z;

        currentJoystickPitch = Mathf.Abs(rawJoystickPitch) < joystickDeadzone ? 0f : rawJoystickPitch;
        currentJoystickRoll = Mathf.Abs(rawJoystickRoll) < joystickDeadzone ? 0f : rawJoystickRoll;
        currentJoystickYaw = Mathf.Abs(joystickYaw) < thumbstickDeadzone ? 0f : joystickYaw;
    }

    private void AdjustGlobalGravity()
    {
        // Nur die Schwerkraft anpassen, wenn der Jet nicht am Boden ist
        if (!groundContactManagerScript.isGrounded)
        {
            // Berechne den Schwerkraft-Multiplikator basierend auf der Geschwindigkeit
            // InverseLerp gibt 1 zurück, wenn currentSpeed 0 ist, und 0, wenn currentSpeed >= minSpeedForZeroGlobalGravity ist.
            float gravityMultiplier = Mathf.InverseLerp(minSpeedForZeroGlobalGravity, 0, currentSpeed);

            // Setze die Y-Komponente der globalen Schwerkraft
            // Beachte, dass Physics.gravity ein Vector3 ist, dessen x und z Komponenten wir nicht ändern wollen.
            Physics.gravity = new Vector3(initialGlobalGravity.x, -maxGlobalGravity * gravityMultiplier, initialGlobalGravity.z);
        }
        else
        {
            // Wenn der Jet am Boden ist, setze die Schwerkraft auf den maximalen Wert, um ihn am Boden zu halten
            // Oder du kannst sie auf den Standardwert zurücksetzen, je nachdem, was du bevorzugst.
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