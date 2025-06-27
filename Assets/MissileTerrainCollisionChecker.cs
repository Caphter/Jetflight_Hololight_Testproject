using UnityEngine;

public class MissileTerrainCollisionChecker : MonoBehaviour
{
    [SerializeField] private MissileManager missileManager;
    [SerializeField] private string missileSide;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Terrain"))
        {
            missileManager.CollidedWithTerrain(missileSide);
        }
    }
}