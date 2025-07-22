using UnityEngine;

public class WheelTriggerContact : MonoBehaviour
{
    public bool isTouchingGround = false;

    static float wheelTerrainCollisionCooldown = 3f;
    private bool wheelTouchedTerrain = false;
    [SerializeField] private PlaneTerrainCollisionCheck planeTerrainCollisionCheckScript;

    private void Update()
    {
        wheelTerrainCollisionCooldown -= Time.deltaTime;

        if(wheelTouchedTerrain && wheelTerrainCollisionCooldown <= 0f)
        {
            planeTerrainCollisionCheckScript.TriggerCrashExternal();
            wheelTouchedTerrain = false; 
            wheelTerrainCollisionCooldown = 3f; 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ground"))
        {
            isTouchingGround = true;
        }
        else if (other.CompareTag("Terrain"))
        {
            if (!wheelTouchedTerrain)
            {
                wheelTouchedTerrain = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ground"))
        {
            isTouchingGround = false;
        }
    }
}