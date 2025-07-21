using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlaneTerrainCollisionCheck : MonoBehaviour
{
    [SerializeField] private AirplaneMovementController airplaneMovementControllerScript;
    [SerializeField] private ScreenFaderVR screenFaderVRScript;
    [SerializeField] private GameObject crashExplosion;
    [SerializeField] private Transform crashExplosionSpawnPoint;
    [SerializeField] private AudioSource engineSound;
    private bool crashTriggered = false;

    private void Start()
    {
        crashTriggered = false;
        airplaneMovementControllerScript.collisionFreeze = false;
    }

    private IEnumerator HandleCrash()
    {
        crashTriggered = true;
        airplaneMovementControllerScript.collisionFreeze = true; // Plane freezt

        if (crashExplosion != null && crashExplosionSpawnPoint != null)
        {
            FindObjectOfType<AudioManager>().Play("Crash_Explosion");
            engineSound.Stop();

            GameObject currentExplosion = Instantiate(crashExplosion, crashExplosionSpawnPoint.position, crashExplosionSpawnPoint.rotation);
            Destroy(currentExplosion, 1.5f);
        }

        yield return new WaitForSeconds(0.5f);

        screenFaderVRScript.StartFadeToBlack();

        yield return new WaitForSeconds(screenFaderVRScript.defaultFadeOutDuration);

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain") && !crashTriggered)
        {
            StartCoroutine(HandleCrash());
        }
    }
}