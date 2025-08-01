using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using UnityEngine.InputSystem;



public class MissileManager : MonoBehaviour

{

    [Header("Missile Settings:")]

    [SerializeField] private List<GameObject> explosionPrefabs;

    [SerializeField] private Transform missileParentInJet;

    [SerializeField] private float flightTimeTillExplosion = 6f;

    [SerializeField] private float missileSpeed = 10f;

    [SerializeField] private float straightFlightDuration = 0.5f;

    [SerializeField] private float turnRate = 5f;

    [SerializeField] private float crosshairTargetDistance = 100f;

    [SerializeField] private Transform alternativeTargetPoint;



    // NEU: Abstand zum Ziel, bei dem die Rakete nicht mehr lenkt

    [SerializeField] private float stopSteeringDistance = 50f;





    [Header("Missiles")]

    [SerializeField] private List<GameObject> allMissiles = new List<GameObject>(4);

    [SerializeField] private List<Transform> missileRespawnPoints = new List<Transform>(4);

    [SerializeField] private List<Material> missileMaterials = new List<Material>(4);

    [SerializeField] private List<ParticleSystem> missileParticleSystems = new List<ParticleSystem>(4);



    private bool rightTriggerPressed = false;

    private bool preventContiniousFiring = false;

    public InputActionReference triggerPressedButton;



    [SerializeField] private TargetingSystem targetingSystem;

    [SerializeField] private CrosshairTargetColliderCheck crosshairTargetColliderCheck;

    [SerializeField] private Transform crosshairTransform;



    private const string DEFAULT_TAG = "Untagged";

    private const string MISSILE_TAG = "Missile";



    private List<bool> missileAvailable = new List<bool> { true, true, true, true };

    private List<bool> missileCollidedWithTerrain = new List<bool> { false, false, false, false };



    private void Awake()

    {

        if (allMissiles.Count != 4 || missileRespawnPoints.Count != 4 || missileMaterials.Count != 4 || missileParticleSystems.Count != 4)

        {

            Debug.LogError("MissileManager: Die Listen für die Raketen müssen genau 4 Elemente enthalten!");

            enabled = false;

            return;

        }



        foreach (GameObject missile in allMissiles)

        {

            if (missile != null)

            {

                missile.tag = DEFAULT_TAG;

            }

        }



        if (targetingSystem == null)

        {

            Debug.LogError("MissileManager: TargetingSystem-Referenz ist nicht zugewiesen!");

            enabled = false;

            return;

        }

        if (crosshairTransform == null)

        {

            Debug.LogError("MissileManager: CrosshairTransform-Referenz ist nicht zugewiesen!");

            enabled = false;

            return;

        }

        if (alternativeTargetPoint == null)

        {

            Debug.LogError("MissileManager: AlternativeTargetPoint-Referenz ist nicht zugewiesen!");

            enabled = false;

            return;

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



    public void CollidedWithTerrain(int missileIndex, bool targetHit)

    {

        if (missileIndex >= 0 && missileIndex < missileCollidedWithTerrain.Count)

        {

            missileCollidedWithTerrain[missileIndex] = true;

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

        for (int i = 0; i < allMissiles.Count; i++)

        {

            if (missileAvailable[i])

            {

                missileAvailable[i] = false;

                missileCollidedWithTerrain[i] = false;

                StartCoroutine(MissileFlight(i));

                allMissiles[i].tag = MISSILE_TAG;

                return;

            }

        }

    }



    private IEnumerator MissileFlight(int missileIndex)

    {

        GameObject missile = allMissiles[missileIndex];

        Material missileMaterial = missileMaterials[missileIndex];

        ParticleSystem missileParticleSystem = missileParticleSystems[missileIndex];

        Transform missileRespawnPoint = missileRespawnPoints[missileIndex];



        missile.transform.parent = null;

        missileParticleSystem.Play();



        FindObjectOfType<AudioManager>()?.Play("Missile_Launch");



        Vector3 targetPosition = Vector3.zero;



        if (targetingSystem.targetingSystemActive)

        {

            if (targetingSystem.isTargetLocked)

            {

                targetPosition = targetingSystem.GetLockedTargetPosition();

            }

            else

            {

                targetPosition = alternativeTargetPoint.position;

            }

        }



        float elapsedTime = 0f;

        while (elapsedTime < flightTimeTillExplosion && !missileCollidedWithTerrain[missileIndex])

        {

            if (targetingSystem.targetingSystemActive)

            {

                if (elapsedTime < straightFlightDuration)

                {

                    // Flug in gerader Linie

                    missile.transform.Translate(Vector3.forward * missileSpeed * Time.deltaTime, Space.Self);

                }

                else

                {

                    // Flug mit Zielverfolgung

                    float distanceToTarget = Vector3.Distance(missile.transform.position, targetPosition);



                    // NEU: Nur lenken, wenn die Rakete noch weit genug vom Ziel entfernt ist

                    if (distanceToTarget > stopSteeringDistance)

                    {

                        Vector3 directionToTarget = (targetPosition - missile.transform.position).normalized;

                        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                        missile.transform.rotation = Quaternion.RotateTowards(missile.transform.rotation, targetRotation, turnRate * Time.deltaTime);

                    }



                    missile.transform.Translate(Vector3.forward * missileSpeed * Time.deltaTime, Space.Self);

                }

            }

            else

            {

                missile.transform.Translate(Vector3.forward * missileSpeed * Time.deltaTime, Space.Self);

            }



            elapsedTime += Time.deltaTime;

            yield return null;

        }



        bool collided = missileCollidedWithTerrain[missileIndex];



        FindObjectOfType<AudioManager>()?.Stop("Missile_Launch");

        missileParticleSystem.Stop();



        if (collided)

        {

            targetingSystem.ReleaseTargetLock();

        }



        Vector3 explosionPosition = missile.transform.position;



        GameObject obj = Instantiate(explosionPrefabs[Random.Range(0, explosionPrefabs.Count)], explosionPosition, Quaternion.identity);

        FindObjectOfType<AudioManager>()?.Play("Missile_Explosion");

        Destroy(obj, 1.5f);



        yield return StartCoroutine(FadeOutMaterial(missileMaterial, 0.25f));



        missile.transform.position = missileRespawnPoint.position;

        missile.transform.rotation = missileRespawnPoint.rotation;

        missile.transform.parent = missileParentInJet;



        missileAvailable[missileIndex] = true;

        missileCollidedWithTerrain[missileIndex] = false;

        missile.tag = DEFAULT_TAG;



        yield return StartCoroutine(FadeInMaterial(missileMaterial, 0.5f));

    }



    private IEnumerator FadeOutMaterial(Material material, float duration)

    {

        Color color = material.color;

        float startAlpha = color.a;



        SetMaterialRenderModeToFade(material);



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



        SetMaterialRenderModeToFade(material);



        for (float t = 0; t < duration; t += Time.deltaTime)

        {

            color.a = Mathf.Lerp(startAlpha, 1f, t / duration);

            material.color = color;

            yield return null;

        }



        color.a = 1f;

        material.color = color;

    }



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