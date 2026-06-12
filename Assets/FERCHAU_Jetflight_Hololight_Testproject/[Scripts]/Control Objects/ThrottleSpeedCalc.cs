using UnityEngine;

/// <summary>
/// Berechnet die aktuelle Geschwindigkeit basierend auf dem Winkel des Throttle-Objekts.
/// </summary>
public class ThrottleSpeedCalc : MonoBehaviour
{
    [SerializeField] private GameObject throttleObject;
    [SerializeField] private float maxSpeedValue;

    public float GetCurrentThrottleToSpeedValue()
    {
        // Berechne den aktuellen Winkel des throttleObjects
        float angle = throttleObject.transform.localEulerAngles.x;

        if (angle > 65f)
        {
            return maxSpeedValue;
        }

        if (angle < 10f)
        {
            return 0f;
        }

        // Interpolation von 0 bis maxSpeedValue zwischen 10 und 65 Grad
        // Normalisiere den Winkel in den Bereich [0, 1]
        float normalizedAngle = (angle - 10f) / (65f - 10f); // Normalisierung auf [0, 1]
        float currentSpeed = Mathf.Lerp(0f, maxSpeedValue, normalizedAngle); // Interpolieren


        return currentSpeed;
    }

    public float GetMaxThrottleToSpeedValue()
    {
        return maxSpeedValue; // Oder wie immer du deine maximale Geschwindigkeit definierst
    }
}
