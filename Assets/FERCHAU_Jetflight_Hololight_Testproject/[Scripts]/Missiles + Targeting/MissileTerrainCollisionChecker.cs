using System.Collections;
using UnityEngine;

public class MissileTerrainCollisionChecker : MonoBehaviour
{
    [SerializeField] private MissileManager missileManager;
    [SerializeField] private int missileNumber;
    [SerializeField] private GameObject hitmarkerGameObject;
    [SerializeField] private float hitmarkerDuration = 1f;
    [SerializeField] private TargetingSystem targetingSystemScript;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Terrain"))
        {
            missileManager.CollidedWithTerrain(missileNumber, false);
        }

        if (other.CompareTag("Target"))
        {
            missileManager.CollidedWithTerrain(missileNumber, true);

            if (targetingSystemScript.targetingSystemActive)
            {
                StartCoroutine(HitmarkerCoroutine());
            }
        }
    }

    private IEnumerator HitmarkerCoroutine()
    {
        hitmarkerGameObject.SetActive(true);
        yield return new WaitForSeconds(hitmarkerDuration);
        hitmarkerGameObject.SetActive(false);
    }
}