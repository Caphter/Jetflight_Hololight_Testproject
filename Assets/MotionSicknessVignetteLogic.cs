using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class MotionSicknessVignetteLogic : MonoBehaviour
{
    [Header("Motion Sickness Vignette Settings")]
    [SerializeField] private PostProcessVolume postProcessVolume; // Referenz zum PostProcessVolume in der Szene
    [SerializeField] private AirplaneMovementController airplaneMovementControllerScript; // Referenz zum PostProcessVolume in der Szene

    [Range(0f, 1f)][SerializeField] private float maxVignetteIntensity = 0.8f; // Maximale Intensität der Vignette
    // Ein einziger Schwellenwert für die "gefühlte" Rotationsstärke, ab der die Vignette maximal wird
    [Range(0.1f, 10f)][SerializeField] private float combinedRotationStrengthThreshold = 1f;
    [Range(0.1f, 5f)][SerializeField] private float vignetteFadeInSpeed = 2f; // Geschwindigkeit, mit der die Vignette einfadet
    [Range(0.1f, 5f)][SerializeField] private float vignetteFadeOutSpeed = 1f; // Geschwindigkeit, mit der die Vignette ausfadet
    [Range(0f, 0.5f)][SerializeField] private float minInputThresholdForVignette = 0.1f; // Mindest-Inputstärke, um Vignette zu aktivieren/halten

    private Vignette vignette; // Referenz zum Vignette-Effekt

    private void Start()
    {
        if (!postProcessVolume.profile.TryGetSettings(out vignette))
        {
            Debug.LogError("Vignette-Effekt im PostProcessVolume-Profil nicht gefunden! Bitte zum Profil hinzufuegen.");
            enabled = false;
            return;
        }

        vignette.intensity.value = 0f;
        vignette.active = true;
    }

    private void Update()
    {
        UpdateVignette();
    }

    private void OnDestroy()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0f;
        }
    }

    private void UpdateVignette()
    {
        // Absolute Werte der Inputs nach Deadzone-Anwendung
        float absPitchInput = Mathf.Abs(airplaneMovementControllerScript.currentJoystickPitch);
        float absRollInput = Mathf.Abs(airplaneMovementControllerScript.currentJoystickRoll);
        float absYawInput = Mathf.Abs(airplaneMovementControllerScript.currentJoystickYaw);

        // Berechne eine "kombinierte Rotationsstärke"
        // Hier gewichten wir die Inputs mit ihren jeweiligen Geschwindigkeiten um die "gefühlte" Stärke der Rotation zu erhalten.

        float maxPossibleSpeed = Mathf.Max(airplaneMovementControllerScript.pitchSpeed, airplaneMovementControllerScript.rollSpeed, airplaneMovementControllerScript.yawSpeed);

        float weightedPitchStrength = absPitchInput * (airplaneMovementControllerScript.pitchSpeed / maxPossibleSpeed);
        float weightedRollStrength = absRollInput * (airplaneMovementControllerScript.rollSpeed / maxPossibleSpeed);
        float weightedYawStrength = absYawInput * (airplaneMovementControllerScript.yawSpeed / maxPossibleSpeed);

        // Summiere die gewichteten Stärken. Clamp01, falls Summe > 1 ist.
        float combinedRotationStrength = Mathf.Clamp01(weightedPitchStrength + weightedRollStrength + weightedYawStrength);


        // Bestimme die Ziel-Vignette-Intensität
        float targetVignetteIntensity = 0f;

        // Wenn eine signifikante Input-Stärke vorhanden ist
        if (combinedRotationStrength > minInputThresholdForVignette)
        {
            // Die Zielintensität hängt von der kombinierten Stärke ab und wird auf den maxVignetteIntensity skaliert
            targetVignetteIntensity = Mathf.Clamp01(combinedRotationStrength / combinedRotationStrengthThreshold) * maxVignetteIntensity;

            // Fade-In: Bewege die aktuelle Intensität *schneller* in Richtung des Zielwerts
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetVignetteIntensity, Time.deltaTime * vignetteFadeInSpeed);
        }
        else
        {
            // Keine signifikante Rotation oder Input, also fade-out zum Nullwert
            // Fade-Out: Bewege die aktuelle Intensität *langsamer* (oder schneller, je nach Einstellung) in Richtung 0
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0f, Time.deltaTime * vignetteFadeOutSpeed);
        }
    }
}
