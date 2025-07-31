using UnityEngine;

public class MissileTerrainCollisionChecker : MonoBehaviour
{
    [SerializeField] private MissileManager missileManager;
    [SerializeField] private int missileNumber;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Terrain") || other.CompareTag("Target"))
        {
            missileManager.CollidedWithTerrain(missileNumber);
        }
    }
}