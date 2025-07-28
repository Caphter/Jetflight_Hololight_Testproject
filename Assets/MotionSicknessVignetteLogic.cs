using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using System.Collections;

/// <summary>
/// Manages the dynamic vignette effect to reduce motion sickness during airplane rotations.
/// The vignette intensity is based on the combined rotation strength from pitch, roll, and yaw inputs.
/// This script acts as an ITunnelingVignetteProvider for the XR Interaction Toolkit's TunnelingVignetteController,
/// allowing the controller to handle the easing in and out effects.
/// </summary>
public class MotionSicknessVignetteLogic : MonoBehaviour, ITunnelingVignetteProvider
{
    // NEU: Referenz zum AirplaneMovementController
    [Header("Airplane Movement References")]
    [Tooltip("Referenz zu Ihrem AirplaneMovementController, um die aktuellen Joystick-Inputs und Rotationsgeschwindigkeiten abzurufen.")]
    [SerializeField] private AirplaneMovementController airplaneMovementController;

    [Header("VR Vignette Settings")]
    [Tooltip("Die minimale Blendenöffnung (0 = volle Vignette, 1 = keine Vignette). Eine kleinere Zahl bedeutet eine stärkere Vignette.")]
    [Range(0f, 1f)][SerializeField] private float minApertureSize = 0.2f;

    [Tooltip("Die kombinierte Rotationsstärke, bei der die Vignette ihre minimale Aperturgröße (maximale Stärke) erreicht.")]
    [Range(0.1f, 10f)][SerializeField] private float combinedRotationStrengthThreshold = 1f;

    [Tooltip("Die Geschwindigkeit, mit der die Vignette eingeblendet wird.")]
    [Range(0.01f, 5f)][SerializeField] private float vignetteEaseInTime = 0.5f;

    [Tooltip("Die Geschwindigkeit, mit der die Vignette ausgeblendet wird.")]
    [Range(0.01f, 5f)][SerializeField] private float vignetteEaseOutTime = 1.0f;

    [Tooltip("Die minimale Input-Stärke (Pitch, Roll, Yaw), ab der die Vignette aktiviert wird.")]
    [Range(0f, 0.5f)][SerializeField] private float minInputThresholdForVignette = 0.1f;

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
    private bool m_isExternallyControlled = false; // Flag to indicate external control
    private Coroutine m_externalVignetteRoutine; // Coroutine for externally controlled vignette

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
        if (m_isExternallyControlled)
        {
            return; // Skip motion-based logic if externally controlled
        }

        // Hole die Inputs vom AirplaneMovementController
        float currentJoystickPitch = airplaneMovementController.CurrentJoystickPitch;
        float currentJoystickRoll = airplaneMovementController.CurrentJoystickRoll;
        float currentJoystickYaw = airplaneMovementController.CurrentJoystickYaw;

        // Hole die Rotationsgeschwindigkeiten (Konstanten) vom AirplaneMovementController
        float pitchSettingSpeed = airplaneMovementController.PitchSpeed;
        float rollSettingSpeed = airplaneMovementController.RollSpeed;
        float yawSettingSpeed = airplaneMovementController.YawSpeed;

        // Absolute Werte der Inputs
        float absPitchInput = Mathf.Abs(currentJoystickPitch);
        float absRollInput = Mathf.Abs(currentJoystickRoll);
        float absYawInput = Mathf.Abs(currentJoystickYaw);

        // Berechne die *gewichteteste einzelne* Rotationsstärke
        float weightedPitchStrength = absPitchInput * pitchSettingSpeed;
        float weightedRollStrength = absRollInput * rollSettingSpeed;
        float weightedYawStrength = absYawInput * yawSettingSpeed;

        // Finde die maximale gewichtete Stärke unter allen Achsen
        float highestWeightedStrength = Mathf.Max(weightedPitchStrength, weightedRollStrength, weightedYawStrength);

        // Normalisiere die höchste gewichtete Stärke relativ zum combinedRotationStrengthThreshold
        // um einen Wert zwischen 0 und 1 zu erhalten, der die Intensität der Vignette steuert.
        float vignetteIntensityFactor = 0f;
        if (combinedRotationStrengthThreshold > 0.001f) // Vermeide Division durch Null
        {
            vignetteIntensityFactor = Mathf.Clamp01(highestWeightedStrength / combinedRotationStrengthThreshold);
        }

        // Bestimme die Ziel-Aperturgröße für die XR Vignette
        float desiredApertureSize;

        // Die Vignette wird aktiviert, wenn der höchste gewichtete Input den Schwellenwert überschreitet
        if (highestWeightedStrength > minInputThresholdForVignette * combinedRotationStrengthThreshold)
        {
            // Die Vignette wird stärker (Aperturgröße kleiner), wenn die Rotation zunimmt
            desiredApertureSize = Mathf.Lerp(VignetteParameters.Defaults.apertureSizeMax, minApertureSize, vignetteIntensityFactor);

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
        if (m_externalVignetteRoutine != null)
        {
            StopCoroutine(m_externalVignetteRoutine);
            m_externalVignetteRoutine = null;
        }
    }

    /// <summary>
    /// Requests a temporary vignette for a specified duration, then releases control.
    /// </summary>
    /// <param name="aperture">The aperture size for this temporary vignette (0 = max strong, 1 = no vignette).</param>
    /// <param name="easeInTime">The time it takes for the vignette to fade in.</param>
    /// <param name="duration">How long the vignette should remain active at its target aperture.</param>
    /// <param name="easeOutTimeOverride">Optional: Override the default easeOutTime when releasing control.</param>
    public void RequestTemporaryVignette(float aperture, float easeInTime, float duration, float easeOutTimeOverride = -1f)
    {
        if (m_externalVignetteRoutine != null)
        {
            StopCoroutine(m_externalVignetteRoutine);
        }
        m_externalVignetteRoutine = StartCoroutine(TemporaryVignetteRoutine(aperture, easeInTime, duration, easeOutTimeOverride));
    }

    private IEnumerator TemporaryVignetteRoutine(float aperture, float easeInTime, float duration, float easeOutTimeOverride)
    {
        m_isExternallyControlled = true; // Take control from Update()

        m_VignetteParameters.apertureSize = aperture;
        m_VignetteParameters.easeInTime = easeInTime;
        m_VignetteParameters.easeOutTime = (easeOutTimeOverride >= 0) ? easeOutTimeOverride : vignetteEaseOutTime; // Use override or default
        m_VignetteParameters.easeOutDelayTime = 0f; // No delay for temporary vignettes
        m_VignetteParameters.easeInTimeLock = false;

        if (!m_isVignetteRequestedActive)
        {
            m_TunnelingVignetteController.BeginTunnelingVignette(this);
            m_isVignetteRequestedActive = true;
        }

        yield return new WaitForSeconds(duration);

        // After the duration, release control, allowing the vignette to fade out
        ReleaseVignetteControl();

        m_externalVignetteRoutine = null; // Clear the coroutine reference
    }

    /// <summary>
    /// Releases external control of the vignette, allowing the motion-based logic to resume
    /// or for the vignette to fade out if no motion is present.
    /// </summary>
    public void ReleaseVignetteControl()
    {
        if (m_externalVignetteRoutine != null)
        {
            StopCoroutine(m_externalVignetteRoutine);
            m_externalVignetteRoutine = null;
        }
        m_isExternallyControlled = false;
        // The EndTunnelingVignette will be handled by Update() if motion is low,
        // or a new vignette request if motion is high.
        // We explicitly end it here to ensure immediate fade-out if Update() isn't actively managing it.
        if (m_isVignetteRequestedActive)
        {
            m_TunnelingVignetteController.EndTunnelingVignette(this);
            m_isVignetteRequestedActive = false;
        }
    }

    /// <summary>
    /// Resets the vignette parameters to their default configured values and releases external control.
    /// </summary>
    public void ResetVignetteToDefaults()
    {
        ReleaseVignetteControl();
        // Resetting to defaults will be handled by the Update() loop based on motion,
        // or you could explicitly set them here if needed.
        m_VignetteParameters.apertureSize = VignetteParameters.Defaults.apertureSizeMax; // Fully open aperture
        m_VignetteParameters.easeInTime = vignetteEaseInTime;
        m_VignetteParameters.easeOutTime = vignetteEaseOutTime;
        m_VignetteParameters.easeOutDelayTime = 0f;
    }
}