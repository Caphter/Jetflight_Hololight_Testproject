using UnityEngine;

public class RaycastVisualizer : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("Länge des Raycasts.")]
    [SerializeField] private float raycastLength = 10f;

    [Tooltip("LayerMask, mit der der Raycast kollidieren soll. Nur Objekte auf diesen Layern werden erkannt.")]
    [SerializeField] private LayerMask hitLayers = -1; // -1 bedeutet "Alles"

    [Tooltip("Farbe des Raycasts, wenn er kein Objekt trifft.")]
    [SerializeField] private Color noHitColor = Color.green;

    [Tooltip("Farbe des Raycasts, wenn er ein Objekt trifft.")]
    [SerializeField] private Color hitColor = Color.red;

    private void Update()
    {
        // Dieser Update-Call ist nur dafür da, um den Raycast auch im Play Mode zu sehen.
        // Die visuelle Darstellung im Editor (ohne Play Mode) wird in OnDrawGizmos/OnDrawGizmosSelected gehandhabt.
        PerformRaycastAndDrawGizmo(transform.position, transform.forward, raycastLength, hitLayers);
    }

    private void OnDrawGizmos()
    {
        // Zeichnet den Raycast im Editor, auch wenn das GameObject nicht ausgewählt ist.
        // Optional, da OnDrawGizmosSelected oft bevorzugt wird.
        PerformRaycastAndDrawGizmo(transform.position, transform.forward, raycastLength, hitLayers);
    }

    private void OnDrawGizmosSelected()
    {
        // Zeichnet den Raycast im Editor NUR, wenn das GameObject ausgewählt ist.
        PerformRaycastAndDrawGizmo(transform.position, transform.forward, raycastLength, hitLayers);
    }

    private void PerformRaycastAndDrawGizmo(Vector3 origin, Vector3 direction, float length, LayerMask layers)
    {
        RaycastHit hit;
        // Führt den Raycast aus
        if (Physics.Raycast(origin, direction, out hit, length, layers))
        {
            // Raycast trifft etwas
            Gizmos.color = hitColor;
            Gizmos.DrawLine(origin, hit.point); // Linie bis zum Trefferpunkt
            Gizmos.DrawSphere(hit.point, 0.1f); // Kleiner Punkt am Trefferpunkt
            // Optional: Verlängere die Linie in der Farbe, wenn sie über den Trefferpunkt hinausgehen soll
            // Gizmos.DrawLine(hit.point, origin + direction * length);
        }
        else
        {
            // Raycast trifft nichts
            Gizmos.color = noHitColor;
            Gizmos.DrawLine(origin, origin + direction * length); // Linie über die gesamte Länge
        }
    }
}