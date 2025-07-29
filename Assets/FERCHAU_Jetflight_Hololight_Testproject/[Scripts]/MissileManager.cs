using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    // NEU: Konstanten für Tags
    private const string DEFAULT_TAG = "Untagged";
    private const string MISSILE_TAG = "Missile";

    private void Awake() // Verwende Awake für die initiale Tag-Setzung
    {
        if (missileRight != null)
        {
            missileRight.tag = DEFAULT_TAG;
        }
        if (missileLeft != null)
        {
            missileLeft.tag = DEFAULT_TAG;
        }
    }

    private void Start()
    {
        if (triggerPressedButton == null)
        {
            Debug.LogError("InputActionReference für triggerPressedButton ist nicht zugewiesen!");
            return;
        }

        triggerPressedButton.action.Enable();
        triggerPressedButton.action.started += TriggerWasPressed;
        triggerPressedButton.action.canceled += TriggerWasReleased;
    }

    private void OnDestroy()
    {
        if (triggerPressedButton != null && triggerPressedButton.action != null)
        {
            triggerPressedButton.action.started -= TriggerWasPressed;
            triggerPressedButton.action.canceled -= TriggerWasReleased;
            triggerPressedButton.action.Disable();
        }
    }

    private void Update()
    {
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
            rightCollidedWithTerrain = false;
            StartCoroutine(MissileFlight(missileRight, missileMaterialRight, "right", missileParticleSystemRight));
            missileRight.tag = MISSILE_TAG; 
        }
        else if (missileLeftAvailable)
        {
            missileLeftAvailable = false;
            leftCollidedWithTerrain = false;
            StartCoroutine(MissileFlight(missileLeft, missileMaterialLeft, "left", missileParticleSystemLeft));
            missileLeft.tag = MISSILE_TAG; 
        }
        else
        {
            return;
        }
    }

    private IEnumerator MissileFlight(GameObject missile, Material missileMaterial, string side, ParticleSystem missileParticleSystem)
    {
        missile.transform.parent = null;
        missileParticleSystem.Play();

        FindObjectOfType<AudioManager>().Play("Missile_Launch");

        // Rakete fliegt für die angegebene Zeit oder bis zur Kollision
        // Die Bewegung der Rakete ist nun direkt in dieser Coroutine enthalten
        float elapsedTime = 0f;
        while (elapsedTime < flightTimeTillExplosion && !(side == "right" ? rightCollidedWithTerrain : leftCollidedWithTerrain))
        {
            missile.transform.Translate(Vector3.forward * missileSpeed * Time.deltaTime); // <-- HIER IST DIE BEWEGUNG!
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        bool collided = (side == "right" ? rightCollidedWithTerrain : leftCollidedWithTerrain);

        FindObjectOfType<AudioManager>().Stop("Missile_Launch");

        missileParticleSystem.Stop();

        Vector3 explosionPosition = side == "right" ? explosionReferencePointRight.position : explosionReferencePointLeft.position;
        if (collided)
        {
            explosionPosition = missile.transform.position;
        }

        GameObject obj = Instantiate(explosionPrefabs[Random.Range(0, explosionPrefabs.Count)], explosionPosition, Quaternion.identity);
        FindObjectOfType<AudioManager>().Play("Missile_Explosion");
        Destroy(obj, 1.5f);

        yield return StartCoroutine(FadeOutMaterial(missileMaterial, 0.25f));

        // Rakete zurück an den Respawn-Punkt setzen
        if (side == "right")
        {
            missile.transform.position = missileRespawnPointRight.position;
            missile.transform.rotation = missileRespawnPointRight.rotation;
            missile.transform.parent = missileParentInJet;
            missileRightAvailable = true;
            rightCollidedWithTerrain = false; // Reset collision flag
            missileRight.tag = DEFAULT_TAG; // Reset right missile tag
        }
        else if (side == "left")
        {
            missile.transform.position = missileRespawnPointLeft.position;
            missile.transform.rotation = missileRespawnPointLeft.rotation;
            missile.transform.parent = missileParentInJet;
            missileLeftAvailable = true;
            leftCollidedWithTerrain = false; // Reset collision flag
            missileLeft.tag = DEFAULT_TAG; // Reset left missile tag
        }

        // Rakete über 0.5s einfaden
        yield return StartCoroutine(FadeInMaterial(missileMaterial, 0.5f));
    }

    // <-- DIE MOVE MISSILE METHODE WURDE ENTFERNT!

    private IEnumerator FadeOutMaterial(Material material, float duration)
    {
        Color color = material.color;
        float startAlpha = color.a;

        SetMaterialRenderModeToFade(material); // Sicherstellen, dass der Render Mode korrekt ist

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

        SetMaterialRenderModeToFade(material); // Sicherstellen, dass der Render Mode korrekt ist

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            color.a = Mathf.Lerp(startAlpha, 1f, t / duration);
            material.color = color;
            yield return null;
        }

        color.a = 1f;
        material.color = color;
    }

    // Hilfsfunktion zur Material-Konfiguration für Transparenz (falls nicht schon vorhanden)
    private void SetMaterialRenderModeToFade(Material material)
    {
        if (material.renderQueue != 3000)
        {
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }
    }
}