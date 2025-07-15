using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Manages the dynamic vignette effect to reduce motion sickness during airplane rotations.
/// The vignette intensity is based on the combined rotation strength from pitch, roll, and yaw inputs.
/// </summary>
public class MotionSicknessVignetteLogic : MonoBehaviour
{
    [Header("Vignette Settings")]
    [SerializeField] private PostProcessVolume postProcessVolume; // Referenz zum PostProcessVolume in der Szene
    [Range(0f, 1f)][SerializeField] private float maxVignetteIntensity = 0.8f; // Maximale Intensität der Vignette
    [Range(0.1f, 10f)][SerializeField] private float combinedRotationStrengthThreshold = 1f; // Schwellenwert für maximale Vignette
    [Range(0.1f, 5f)][SerializeField] private float vignetteFadeInSpeed = 2f; // Geschwindigkeit des Einfadens
    [Range(0.1f, 5f)][SerializeField] private float vignetteFadeOutSpeed = 1f; // Geschwindigkeit des Ausfadens
    [Range(0f, 0.5f)][SerializeField] private float minInputThresholdForVignette = 0.1f; // Mindest-Inputstärke für Vignette

    private Vignette vignette; // Referenz zum Vignette-Effekt

    private void Start()
    {
        if (postProcessVolume == null)
        {
            Debug.LogError("PostProcessVolume ist nicht zugewiesen! Bitte im Inspector zuweisen.");
            enabled = false;
            return;
        }

        if (!postProcessVolume.profile.TryGetSettings(out vignette))
        {
            Debug.LogError("Vignette-Effekt im PostProcessVolume-Profil nicht gefunden! Bitte zum Profil hinzufügen.");
            enabled = false;
            return;
        }

        vignette.intensity.value = 0f;
        vignette.active = true;
    }

    private void OnDestroy()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0f;
        }
    }

    /// <summary>
    /// Updates the vignette intensity based on the provided joystick input values and rotation speeds.
    /// </summary>
    /// <param name="currentJoystickPitch">Current pitch input after deadzone</param>
    /// <param name="currentJoystickRoll">Current roll input after deadzone</param>
    /// <param name="currentJoystickYaw">Current yaw input after deadzone</param>
    /// <param name="pitchSpeed">Pitch rotation speed</param>
    /// <param name="rollSpeed">Roll rotation speed</param>
    /// <param name="yawSpeed">Yaw rotation speed</param>
    public void UpdateVignette(float currentJoystickPitch, float currentJoystickRoll, float currentJoystickYaw,
                              float pitchSpeed, float rollSpeed, float yawSpeed)
    {
        // Absolute Werte der Inputs
        float absPitchInput = Mathf.Abs(currentJoystickPitch);
        float absRollInput = Mathf.Abs(currentJoystickRoll);
        float absYawInput = Mathf.Abs(currentJoystickYaw);

        // Berechne kombinierte Rotationsstärke
        float maxPossibleSpeed = Mathf.Max(pitchSpeed, rollSpeed, yawSpeed);

        float weightedPitchStrength = absPitchInput * (pitchSpeed / maxPossibleSpeed);
        float weightedRollStrength = absRollInput * (rollSpeed / maxPossibleSpeed);
        float weightedYawStrength = absYawInput * (yawSpeed / maxPossibleSpeed);

        float combinedRotationStrength = Mathf.Clamp01(weightedPitchStrength + weightedRollStrength + weightedYawStrength);

        // Bestimme die Ziel-Vignette-Intensität
        float targetVignetteIntensity = 0f;

        if (combinedRotationStrength > minInputThresholdForVignette)
        {
            targetVignetteIntensity = Mathf.Clamp01(combinedRotationStrength / combinedRotationStrengthThreshold) * maxVignetteIntensity;
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetVignetteIntensity, Time.deltaTime * vignetteFadeInSpeed);
        }
        else
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0f, Time.deltaTime * vignetteFadeOutSpeed);
        }
    }
}