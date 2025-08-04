using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenFaderVR : MonoBehaviour
{
    [HideInInspector] public bool inBlendingProcess = false;

    [Header("Transition Sphere Settings")]
    [SerializeField] private Renderer transitionSphereRenderer;
    public float defaultFadeOutDuration = 1f;
    public float defaultFadeInDuration = 1f; // Default duration for fade in and out
    [SerializeField] private bool sceneIsStartingBlack = false;

    private void Start()
    {

        if (sceneIsStartingBlack)
        {
            transitionSphereRenderer.material.color = new Color(0f, 0f, 0f, 1f);
            StartFadeFromBlack();
        }
    }

    public IEnumerator FadeToBlack(float duration = -1f, bool shouldRestartScene = false)
    {
        if (transitionSphereRenderer == null)
        {
            Debug.LogError("Transition Sphere Renderer not assigned! Cannot fade to black.");
            yield break;
        }

        if (inBlendingProcess)
        {
            Debug.LogWarning("FadeToBlack called while blending is already in progress. Ignoring.");
            yield break;
        }

        inBlendingProcess = true;

        float actualDuration = (duration > 0) ? duration : defaultFadeOutDuration;
        Color startColor = transitionSphereRenderer.material.color;
        Color targetColor = new Color(0, 0, 0, 1f); // Fully black

        for (float t = 0; t < actualDuration; t += Time.deltaTime) 
        {
            float blendValue = t / actualDuration;
            transitionSphereRenderer.material.color = Color.Lerp(startColor, targetColor, blendValue);
            yield return null;
        }
        transitionSphereRenderer.material.color = targetColor;

        inBlendingProcess = false;

        if (shouldRestartScene)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public IEnumerator FadeFromBlack(float duration = -1f)
    {
        if (transitionSphereRenderer == null)
        {
            Debug.LogError("Transition Sphere Renderer not assigned! Cannot fade from black.");
            yield break;
        }

        if (inBlendingProcess)
        {
            Debug.LogWarning("FadeFromBlack called while blending is already in progress. Ignoring.");
            yield break;
        }

        inBlendingProcess = true;

        float actualDuration = (duration > 0) ? duration : defaultFadeInDuration;
        Color startColor = transitionSphereRenderer.material.color;
        Color targetColor = new Color(0, 0, 0, 0f); // Fully transparent

        for (float t = 0; t < actualDuration; t += Time.deltaTime) 
        {
            float blendValue = t / actualDuration;
            transitionSphereRenderer.material.color = Color.Lerp(startColor, targetColor, blendValue);
            yield return null;
        }
        transitionSphereRenderer.material.color = targetColor;

        inBlendingProcess = false;
    }

    public void StartFadeToBlack(float duration = -1f, bool shouldRestartScene = false)
    {
        StartCoroutine(FadeToBlack(duration, shouldRestartScene));
    }

    public void StartFadeFromBlack(float duration = -1f)
    {
        StartCoroutine(FadeFromBlack(duration));
    }
}