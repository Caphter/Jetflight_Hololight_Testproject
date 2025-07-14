using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Controls the movement of the airplane based on joystick input. It calculates the speed of the airplane and moves it forward.
/// The airplane can be rotated around the x, y, and z axes (Pitch, Roll, and Yaw).
/// Also manages a dynamic vignette to reduce motion sickness during rotations with customizable fade in/out times.
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
    private float joystickYaw; // Wert von -1 (links) bis 1 (rechts)

    // Speichern der aktuellen Joystick/Thumbstick Inputs nach Deadzone-Anwendung
    private float currentJoystickPitch;
    private float currentJoystickRoll;
    private float currentJoystickYaw;

    public float currentSpeed; // Calculated speed

    [Header("Motion Sickness Vignette Settings")]
    [SerializeField] private PostProcessVolume postProcessVolume; // Referenz zum PostProcessVolume in der Szene
    [Range(0f, 1f)][SerializeField] private float maxVignetteIntensity = 0.8f; // Maximale Intensit�t der Vignette
    // Ein einziger Schwellenwert f�r die "gef�hlte" Rotationsst�rke, ab der die Vignette maximal wird
    [Range(0.1f, 10f)][SerializeField] private float combinedRotationStrengthThreshold = 1f;
    [Range(0.1f, 5f)][SerializeField] private float vignetteFadeInSpeed = 2f; // Geschwindigkeit, mit der die Vignette einfadet
    [Range(0.1f, 5f)][SerializeField] private float vignetteFadeOutSpeed = 1f; // Geschwindigkeit, mit der die Vignette ausfadet
    [Range(0f, 0.5f)][SerializeField] private float minInputThresholdForVignette = 0.1f; // Mindest-Inputst�rke, um Vignette zu aktivieren/halten

    private Vignette vignette; // Referenz zum Vignette-Effekt
    // lastRotation wird nicht mehr ben�tigt, da wir Inputs nutzen

    private void Start()
    {
        rightThumbstick.action.performed += RightThumbstickMoved;
        rightThumbstick.action.canceled += RightThumbstickReleased;

        if (postProcessVolume == null)
        {
            Debug.LogError("PostProcessVolume ist nicht zugewiesen! Bitte im Inspector zuweisen.");
            enabled = false;
            return;
        }

        if (!postProcessVolume.profile.TryGetSettings(out vignette))
        {
            Debug.LogError("Vignette-Effekt im PostProcessVolume-Profil nicht gefunden! Bitte zum Profil hinzuf�gen.");
            enabled = false;
            return;
        }

        vignette.intensity.value = 0f;
        vignette.active = true;
        // lastRotation = jetObject.transform.rotation; // Wird nicht mehr ben�tigt
    }

    private void OnDestroy()
    {
        rightThumbstick.action.performed -= RightThumbstickMoved;
        rightThumbstick.action.canceled -= RightThumbstickReleased;

        if (vignette != null)
        {
            vignette.intensity.value = 0f;
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
        // Speichern der Werte in tempor�ren Variablen, bevor sie an Rotation angewendet werden
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
        // Absolute Werte der Inputs nach Deadzone-Anwendung
        float absPitchInput = Mathf.Abs(currentJoystickPitch);
        float absRollInput = Mathf.Abs(currentJoystickRoll);
        float absYawInput = Mathf.Abs(currentJoystickYaw);

        // Berechne eine "kombinierte Rotationsst�rke"
        // Hier gewichten wir die Inputs mit ihren jeweiligen Geschwindigkeiten,
        // um die "gef�hlte" St�rke der Rotation zu erhalten.
        // Wir normieren die Geschwindigkeiten auf einen "maximalen Input von 1.0".
        // Nehmen wir an, die h�chste Geschwindigkeit ist die Roll Speed.
        float maxPossibleSpeed = Mathf.Max(pitchSpeed, rollSpeed, yawSpeed);

        float weightedPitchStrength = absPitchInput * (pitchSpeed / maxPossibleSpeed);
        float weightedRollStrength = absRollInput * (rollSpeed / maxPossibleSpeed);
        float weightedYawStrength = absYawInput * (yawSpeed / maxPossibleSpeed);

        // Summiere die gewichteten St�rken. Clamp01, falls Summe > 1 ist.
        float combinedRotationStrength = Mathf.Clamp01(weightedPitchStrength + weightedRollStrength + weightedYawStrength);


        // Bestimme die Ziel-Vignette-Intensit�t
        float targetVignetteIntensity = 0f;

        // Wenn eine signifikante Input-St�rke vorhanden ist
        if (combinedRotationStrength > minInputThresholdForVignette)
        {
            // Die Zielintensit�t h�ngt von der kombinierten St�rke ab und wird auf den maxVignetteIntensity skaliert
            targetVignetteIntensity = Mathf.Clamp01(combinedRotationStrength / combinedRotationStrengthThreshold) * maxVignetteIntensity;

            // Fade-In: Bewege die aktuelle Intensit�t *schneller* in Richtung des Zielwerts
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetVignetteIntensity, Time.deltaTime * vignetteFadeInSpeed);
        }
        else
        {
            // Keine signifikante Rotation oder Input, also fade-out zum Nullwert
            // Fade-Out: Bewege die aktuelle Intensit�t *langsamer* (oder schneller, je nach Einstellung) in Richtung 0
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0f, Time.deltaTime * vignetteFadeOutSpeed);
        }
    }
}