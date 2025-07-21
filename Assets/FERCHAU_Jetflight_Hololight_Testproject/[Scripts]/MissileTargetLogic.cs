using UnityEngine;
using System.Collections;

public class MissileTargetLogic : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 30f;

    [Header("Wave Movement Settings")]
    [SerializeField] private float waveSpeed = 1f;
    [SerializeField] private float maxDisplacement = 0.5f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private Color fadeTargetColor = new Color(1f, 1f, 1f, 0f);

    private Vector3 initialPosition;
    private MeshRenderer meshRenderer;
    private Material targetMaterial;
    private bool isFading = false;

    private Color initialColor;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        targetMaterial = new Material(meshRenderer.material);
        meshRenderer.material = targetMaterial;

        SetMaterialRenderModeToFade(targetMaterial);

        initialColor = targetMaterial.color;

        initialPosition = transform.position;
    }

    void Update()
    {
        if (!isFading)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            float newY = initialPosition.y + Mathf.Sin(Time.time * waveSpeed) * maxDisplacement;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Missile") && !isFading)
        {
            FindObjectOfType<AudioManager>().Play("Target_Hit");
            StartCoroutine(FadeOutAndDisable());
        }
    }

    IEnumerator FadeOutAndDisable()
    {
        isFading = true;
        float timer = 0f;

        Color startColor = targetMaterial.color;

        while (timer < fadeDuration)
        {
            targetMaterial.color = Color.Lerp(startColor, fadeTargetColor, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        targetMaterial.color = fadeTargetColor;

        gameObject.SetActive(false);

        isFading = false;
    }

    private void SetMaterialRenderModeToFade(Material material)
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