using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This Unity script manages two missiles (left and right) fired via the right VR controller trigger, moving them at a set speed, exploding on collision with terrain (set by external script) or after a fixed flight time,
/// and respawning them with fade-in/out effects. It uses coroutines to handle missile flight, explosion instantiation, and material fading, tracking availability for sequential firing.
/// </summary>
public class MissileManager : MonoBehaviour
{
    [Header("Missile Settings:")]
    [SerializeField] private List<GameObject> explosionPrefabs;
    [SerializeField] private Transform missileParentInJet;
    [SerializeField] private float flightTimeTillExplosion = 3f;
    [SerializeField] private float missileSpeed = 10f;

    [Header("Missile Right:")]
    [SerializeField] private GameObject missileRight;
    [SerializeField] private Transform explosionReferencePointRight;
    [SerializeField] private Transform missileRespawnPointRight;
    [SerializeField] private Material missileMaterialRight;
    [SerializeField] private ParticleSystem missileParticleSystemRight;
    [SerializeField] private bool missileRightAvailable = true;
    public bool rightCollidedWithTerrain = false;

    [Header("Missile Left:")]
    [SerializeField] private GameObject missileLeft;
    [SerializeField] private Transform explosionReferencePointLeft;
    [SerializeField] private Transform missileRespawnPointLeft;
    [SerializeField] private Material missileMaterialLeft;
    [SerializeField] private ParticleSystem missileParticleSystemLeft;
    [SerializeField] private bool missileLeftAvailable = true;
    public bool leftCollidedWithTerrain = false;

    private bool rightTriggerPressed = false;
    private bool preventContiniousFiring = false;
    public InputActionReference triggerPressedButton;

    private void Start()
    {
        // Prüfen, ob triggerPressedButton zugewiesen ist
        if (triggerPressedButton == null)
        {
            Debug.LogError("InputActionReference für triggerPressedButton ist nicht zugewiesen!");
            return;
        }

        // Input Action explizit aktivieren
        triggerPressedButton.action.Enable();
        triggerPressedButton.action.started += TriggerWasPressed;
        triggerPressedButton.action.canceled += TriggerWasReleased;

        missileLeft.tag = "Untagged";
        missileRight.tag = "Untagged";
    }


    private void Update()
    {
        // Wenn der Trigger gedrückt ist und noch nicht als gedrückt registriert wurde
        if (rightTriggerPressed && !preventContiniousFiring)
        {
            preventContiniousFiring = true;
            FireMissile();
        }
    }

    public void CollidedWithTerrain(string side)
    {
        if (side == "right")
        {
            rightCollidedWithTerrain = true;
        }
        else if (side == "left")
        {
            leftCollidedWithTerrain = true;
        }
    }

    private void TriggerWasPressed(InputAction.CallbackContext context)
    {
        rightTriggerPressed = true;
    }

    private void TriggerWasReleased(InputAction.CallbackContext context)
    {
        rightTriggerPressed = false;
        preventContiniousFiring = false;
    }

    public void FireMissile()
    {
        if (missileRightAvailable)
        {
            missileRightAvailable = false;
            rightCollidedWithTerrain = false; // Reset collision flag
            StartCoroutine(MissileFlight(missileRight, missileMaterialRight, "right", missileParticleSystemRight));
            missileRight.tag = "Missile"; // Set tag to Missile for collision detection
        }
        else if (missileLeftAvailable)
        {
            missileLeftAvailable = false;
            leftCollidedWithTerrain = false; // Reset collision flag
            StartCoroutine(MissileFlight(missileLeft, missileMaterialLeft, "left", missileParticleSystemLeft));
            missileLeft.tag = "Missile"; // Set tag to Missile for collision detection
        }
        else
        {
            return;
        }
    }

    private IEnumerator MissileFlight(GameObject missile, Material missileMaterial, string side, ParticleSystem missileParticleSystem)
    {
        // Rakete aus der Jet-Hierarchie lösen und starten
        missile.transform.parent = null;
        missileParticleSystem.Play();

        FindObjectOfType<AudioManager>().Play("Missile_Launch");

        StartCoroutine(MoveMissile(missile, side));

        // Rakete fliegt für die angegebene Zeit oder bis zur Kollision
        float elapsedTime = 0f;
        bool collided = false;
        while (elapsedTime < flightTimeTillExplosion && !(side == "right" ? rightCollidedWithTerrain : leftCollidedWithTerrain))
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Check if loop exited due to collision
        collided = (side == "right" ? rightCollidedWithTerrain : leftCollidedWithTerrain);

        FindObjectOfType<AudioManager>().Stop("Missile_Launch");

        // Stop particle system
        missileParticleSystem.Stop();

        // Explosion instanziieren
        Vector3 explosionPosition = side == "right" ? explosionReferencePointRight.position : explosionReferencePointLeft.position;
        if (collided)
        {
            // Use missile's current position for explosion on collision
            explosionPosition = missile.transform.position;
        }

        GameObject obj = Instantiate(explosionPrefabs[Random.Range(0, explosionPrefabs.Count)], explosionPosition, Quaternion.identity);
        FindObjectOfType<AudioManager>().Play("Missile_Explosion");
        Destroy(obj, 1.5f);

        // Rakete über 0.25s ausfaden
        yield return StartCoroutine(FadeOutMaterial(missileMaterial, 0.25f));

        // Rakete zurück an den Respawn-Punkt setzen
        if (side == "right")
        {
            missile.transform.position = missileRespawnPointRight.position;
            missile.transform.rotation = missileRespawnPointRight.rotation;
            missile.transform.parent = missileParentInJet;
            missileRightAvailable = true;
            rightCollidedWithTerrain = false; // Reset collision flag
            missileRight.tag = "Untagged"; // Reset right missile tag
        }
        else if (side == "left")
        {
            missile.transform.position = missileRespawnPointLeft.position;
            missile.transform.rotation = missileRespawnPointLeft.rotation;
            missile.transform.parent = missileParentInJet;
            missileLeftAvailable = true;
            leftCollidedWithTerrain = false; // Reset collision flag
            missileLeft.tag = "Untagged"; // Reset left missile tag
        }

        // Rakete über 0.5s einfaden
        yield return StartCoroutine(FadeInMaterial(missileMaterial, 0.5f));
    }

    private IEnumerator MoveMissile(GameObject missile, string side)
    {
        float flightTime = 0f;

        while (flightTime < flightTimeTillExplosion && !(side == "right" ? rightCollidedWithTerrain : leftCollidedWithTerrain))
        {
            missile.transform.Translate(Vector3.forward * missileSpeed * Time.deltaTime);
            flightTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeOutMaterial(Material material, float duration)
    {
        Color color = material.color;
        float startAlpha = color.a;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            color.a = Mathf.Lerp(startAlpha, 0f, t / duration);
            material.color = color;
            yield return null;
        }

        color.a = 0f;
        material.color = color;
    }

    private IEnumerator FadeInMaterial(Material material, float duration)
    {
        Color color = material.color;
        float startAlpha = color.a;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            color.a = Mathf.Lerp(startAlpha, 1f, t / duration);
            material.color = color;
            yield return null;
        }

        color.a = 1f;
        material.color = color;
    }
}