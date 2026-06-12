using UnityEngine;

public class NoTargetingCollisionCheck : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float raycastDistance = 1000f;
    [SerializeField] private LayerMask raycastLayer;
    [SerializeField] private Transform defaultTargetPoint;

    // Die Public Variable, die den Kollisionspunkt speichert
    
    public Vector3 currentCollidingPoint;


    private void Update()
    {
        RaycastHit hit;

        // Führt einen Raycast vom Skript-Objekt in dessen Blickrichtung aus
        if (Physics.Raycast(transform.position, transform.forward, out hit, raycastDistance, raycastLayer))
        {
            // Wenn der Raycast etwas trifft, aktualisiere den Zielpunkt auf den Trefferpunkt
            currentCollidingPoint = hit.point;
        }
        else
        {
            // Wenn nichts getroffen wird, setze den Zielpunkt auf eine Position am Ende des Raycasts
            currentCollidingPoint = defaultTargetPoint.position;
        }
    }
}