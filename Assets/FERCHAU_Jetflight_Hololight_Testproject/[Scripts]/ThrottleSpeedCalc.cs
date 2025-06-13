using UnityEngine;

/// <summary>
/// Berechnet die aktuelle Geschwindigkeit basierend auf dem Winkel des Throttle-Objekts.
/// </summary>
public class ThrottleSpeedCalc : MonoBehaviour
{
    [SerializeField] private GameObject throttleObject;
    [SerializeField] private float maxSpeedValue;

    [Header("Thruster-Flames")]
    [SerializeField] private ParticleSystem thrusterFlame1;
    [SerializeField] private ParticleSystem thrusterFlame2;
    [SerializeField] private float startLifetimeMin = 0.5f;
    [SerializeField] private float startLifetimeMax = 1f;
    [SerializeField] private float startSpeedMin = 0.25f;
    [SerializeField] private float startSpeedMax = 2f;

    public float GetCurrentThrottleToSpeedValue()
    {
        // Berechne den aktuellen Winkel des throttleObjects
        float angle = throttleObject.transform.localEulerAngles.x;

        if (angle > 65f)
        {
            SetThrusterFlames(maxSpeedValue);
            return maxSpeedValue;
        }

        if (angle < 10f)
        {
            SetThrusterFlames(0f);
            return 0f;
        }

        // Interpolation von 0 bis maxSpeedValue zwischen 10 und 65 Grad
        // Normalisiere den Winkel in den Bereich [0, 1]
        float normalizedAngle = (angle - 10f) / (65f - 10f); // Normalisierung auf [0, 1]
        float currentSpeed = Mathf.Lerp(0f, maxSpeedValue, normalizedAngle); // Interpolieren

        SetThrusterFlames(currentSpeed);

        return currentSpeed;
    }

    private void SetThrusterFlames(float currentSpeed)
    {
        if (currentSpeed <= 0f)
        {
            // Deaktiviere das Partikelsystem bei Geschwindigkeit 0
            if (thrusterFlame1.isPlaying) thrusterFlame1.Stop();
            if (thrusterFlame2.isPlaying) thrusterFlame2.Stop();
            return;
        }

        // Aktiviere das Partikelsystem, wenn die Geschwindigkeit größer als 0 ist
        if (!thrusterFlame1.isPlaying) thrusterFlame1.Play();
        if (!thrusterFlame2.isPlaying) thrusterFlame2.Play();

        // Normalisiere die aktuelle Geschwindigkeit auf [0, 1]
        float normalizedSpeed = currentSpeed / maxSpeedValue;

        // Berechne Start Lifetime und Start Speed basierend auf normalisierter Geschwindigkeit
        float currentLifetime = Mathf.Lerp(startLifetimeMin, startLifetimeMax, normalizedSpeed);
        float currentSpeedValue = Mathf.Lerp(startSpeedMin, startSpeedMax, normalizedSpeed);

        // Setze die Lifetime und Geschwindigkeit für das erste Thruster-Particle-System
        var flame1Main = thrusterFlame1.main;
        flame1Main.startLifetime = currentLifetime;
        flame1Main.startSpeed = currentSpeedValue;

        // Setze die Lifetime und Geschwindigkeit für das zweite Thruster-Particle-System
        var flame2Main = thrusterFlame2.main;
        flame2Main.startLifetime = currentLifetime;
        flame2Main.startSpeed = currentSpeedValue;
    }
}
