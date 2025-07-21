using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

/// <summary>
/// Manages the dynamic vignette effect to reduce motion sickness during airplane rotations.
/// The vignette intensity is based on the combined rotation strength from pitch, roll, and yaw inputs.
/// This script acts as an ITunnelingVignetteProvider for the XR Interaction Toolkit's TunnelingVignetteController,
/// allowing the controller to handle the easing in and out effects.
/// </summary>
public class MotionSicknessVignetteLogic : MonoBehaviour, ITunnelingVignetteProvider // <--- Implementiere das Interface!
{
    // NEU: Referenz zum AirplaneMovementController
    [Header("Airplane Movement References")]
    [Tooltip("Referenz zu Ihrem AirplaneMovementController, um die aktuellen Joystick-Inputs und Rotationsgeschwindigkeiten abzurufen.")]
    [SerializeField] private AirplaneMovementController airplaneMovementController;

    [Header("VR Vignette Settings")]
    [Tooltip("Die minimale Blendenöffnung (0 = volle Vignette, 1 = keine Vignette). Eine kleinere Zahl bedeutet eine stärkere Vignette.")]
    [Range(0f, 1f)][SerializeField] private float minApertureSize = 0.2f; // Kleinste Blende (stärkster Vignettierungseffekt)

    [Tooltip("Die kombinierte Rotationsstärke, bei der die Vignette ihre minimale Aperturgröße (maximale Stärke) erreicht.")]
    [Range(0.1f, 10f)][SerializeField] private float combinedRotationStrengthThreshold = 1f; // Schwellenwert für maximale Vignette

    [Tooltip("Die Geschwindigkeit, mit der die Vignette eingeblendet wird.")]
    [Range(0.01f, 5f)][SerializeField] private float vignetteEaseInTime = 0.5f; // Easing-In Zeit

    [Tooltip("Die Geschwindigkeit, mit der die Vignette ausgeblendet wird.")]
    [Range(0.01f, 5f)][SerializeField] private float vignetteEaseOutTime = 1.0f; // Easing-Out Zeit

    [Tooltip("Die minimale Input-Stärke (Pitch, Roll, Yaw), ab der die Vignette aktiviert wird.")]
    [Range(0f, 0.5f)][SerializeField] private float minInputThresholdForVignette = 0.1f; // Mindest-Inputstärke für Vignette

    [Header("Additional Vignette Visuals")]
    [Range(0f, 1f)][SerializeField] private float featheringEffect = 0.2f;
    [SerializeField] private Color vignetteColor = Color.black;
    [SerializeField] private Color vignetteColorBlend = Color.black;
    [Range(-0.2f, 0.2f)][SerializeField] private float apertureVerticalPosition = 0f;

    // Referenz auf den TunnelingVignetteController in der Szene
    private TunnelingVignetteController m_TunnelingVignetteController;
    private VignetteParameters m_VignetteParameters; // Unsere eigenen Parameter

    // INTERFACE IMPLEMENTIERUNG: Der Controller fragt diese Eigenschaft ab
    public VignetteParameters vignetteParameters => m_VignetteParameters;

    // Interner Status, um Begin/End nur bei Wechsel aufzurufen
    private bool m_isVignetteRequestedActive = false;

    private void Awake()
    {
        // Finde den TunnelingVignetteController in der Szene.
        m_TunnelingVignetteController = FindObjectOfType<TunnelingVignetteController>();
        if (m_TunnelingVignetteController == null)
        {
            Debug.LogError("TunnelingVignetteController nicht in der Szene gefunden! Bitte stellen Sie sicher, dass einer vorhanden ist (z.B. auf dem XR Origin).");
            enabled = false;
            return;
        }

        // Stelle sicher, dass der AirplaneMovementController zugewiesen ist
        if (airplaneMovementController == null)
        {
            Debug.LogError("AirplaneMovementController ist nicht zugewiesen! Bitte im Inspector zuweisen.");
            enabled = false;
            return;
        }

        // Initialisiere unsere VignetteParameters mit deinen benutzerdefinierten Werten.
        m_VignetteParameters = new VignetteParameters
        {
            featheringEffect = featheringEffect,
            easeInTime = vignetteEaseInTime,
            easeOutTime = vignetteEaseOutTime,
            // Diese bleiben meist fest für kontinuierliche Vignette:
            easeInTimeLock = false,
            easeOutDelayTime = 0f,
            vignetteColor = vignetteColor,
            vignetteColorBlend = vignetteColorBlend,
            apertureVerticalPosition = apertureVerticalPosition
        };

        // Initial setze die Aperturgröße auf den maximalen Wert (keine Vignette)
        m_VignetteParameters.apertureSize = VignetteParameters.Defaults.apertureSizeMax;
    }

    private void Update()
    {
        // Hole die Inputs und Geschwindigkeiten vom AirplaneMovementController
        float currentJoystickPitch = airplaneMovementController.CurrentJoystickPitch;
        float currentJoystickRoll = airplaneMovementController.CurrentJoystickRoll;
        float currentJoystickYaw = airplaneMovementController.CurrentJoystickYaw;

        // Hole die Rotationsgeschwindigkeiten (Konstanten) vom AirplaneMovementController
        // Die *tatsächliche* Rotationsgeschwindigkeit ist normalerweise nicht direkt verfügbar
        // als einfacher float in diesen Scripts, da sie durch Time.deltaTime und die Rotationseinstellungen
        // des Flugzeugs bestimmt wird. Hier verwenden wir die Einstellungs-Geschwindigkeiten
        // als Proxy für die "potentialle" Rotationsgeschwindigkeit, die zu Motion Sickness führen könnte.
        float pitchSettingSpeed = airplaneMovementController.PitchSpeed;
        float rollSettingSpeed = airplaneMovementController.RollSpeed;
        float yawSettingSpeed = airplaneMovementController.YawSpeed;

        // Absolute Werte der Inputs
        float absPitchInput = Mathf.Abs(currentJoystickPitch);
        float absRollInput = Mathf.Abs(currentJoystickRoll);
        float absYawInput = Mathf.Abs(currentJoystickYaw);

        // Berechne kombinierte Rotationsstärke
        // Jetzt verwenden wir die *Einstellungsgeschwindigkeiten* für die Gewichtung.
        // Die Annahme ist, dass höhere Einstellungsgeschwindigkeiten auch zu einem "schlimmeren" Gefühl führen.
        float maxSettingSpeed = Mathf.Max(pitchSettingSpeed, rollSettingSpeed, yawSettingSpeed);
        float combinedRotationStrength = 0f;

        if (maxSettingSpeed > 0.001f) // Vermeide Division durch Null
        {
            // Gewichtung der Inputs mit den Einstellungs-Geschwindigkeiten
            float weightedPitchStrength = absPitchInput * (pitchSettingSpeed / maxSettingSpeed);
            float weightedRollStrength = absRollInput * (rollSettingSpeed / maxSettingSpeed);
            float weightedYawStrength = absYawInput * (yawSettingSpeed / maxSettingSpeed);

            // Normiere die kombinierte Stärke auf 0-1
            combinedRotationStrength = Mathf.Clamp01(weightedPitchStrength + weightedRollStrength + weightedYawStrength);
        }
        else
        {
            combinedRotationStrength = 0f;
        }

        // Bestimme die Ziel-Aperturgröße für die XR Vignette
        float desiredApertureSize;

        if (combinedRotationStrength > minInputThresholdForVignette)
        {
            // Die Vignette wird stärker (Aperturgröße kleiner), wenn die Rotation zunimmt
            float vignetteStrengthNormalized = Mathf.Clamp01(combinedRotationStrength / combinedRotationStrengthThreshold);
            desiredApertureSize = Mathf.Lerp(VignetteParameters.Defaults.apertureSizeMax, minApertureSize, vignetteStrengthNormalized);

            // Setze die Aperturgröße in UNSEREN VignetteParameters
            m_VignetteParameters.apertureSize = desiredApertureSize;

            // Fordere die Vignette beim Controller an
            if (!m_isVignetteRequestedActive)
            {
                m_TunnelingVignetteController.BeginTunnelingVignette(this);
                m_isVignetteRequestedActive = true;
            }
        }
        else
        {
            // Wenn keine signifikante Rotation, beende die Vignette
            if (m_isVignetteRequestedActive)
            {
                m_TunnelingVignetteController.EndTunnelingVignette(this);
                m_isVignetteRequestedActive = false;
            }
        }
    }

    private void OnDisable()
    {
        // Beim Deaktivieren des Skripts oder des GameObjects sicherstellen,
        // dass die Vignette ausgeblendet wird.
        if (m_TunnelingVignetteController != null && m_isVignetteRequestedActive)
        {
            m_TunnelingVignetteController.EndTunnelingVignette(this);
            m_isVignetteRequestedActive = false;
        }
    }
}