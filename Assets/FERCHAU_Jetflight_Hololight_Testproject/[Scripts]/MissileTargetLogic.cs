using System.Collections;
using UnityEngine;

public class MissileTargetLogic : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 30f; // Rotationsgeschwindigkeit in Grad pro Sekunde

    [Header("Wave Movement Settings")]
    [SerializeField] private float waveSpeed = 1f; // Geschwindigkeit der Sinusbewegung
    [SerializeField] private float maxDisplacement = 0.5f; // Maximale Auslenkung der Bewegung

    [Header("Respawn Settings")]
    [SerializeField] private GameObject spawnArea; // GameObject (z. B. Cube), das den Spawn-Bereich definiert
    [SerializeField] private float respawnDelay = 2f; // Zeit bis zur erneuten Sichtbarkeit
    [SerializeField] private float fadeDuration = 0.5f; // Dauer des Fade-Effekts in Sekunden

    private Vector3 initialPosition; // Ursprüngliche Position für die Wellenbewegung
    private MeshRenderer meshRenderer; // Für Zugriff auf das Material
    private Material targetMaterial; // Eigene Materialinstanz für diese Zielscheibe
    private bool isFading = false; // Verhindert gleichzeitige Fade-Vorgänge
    private Bounds spawnBounds; // Grenzen des Spawn-Bereichs

    void Start()
    {
        // Komponenten initialisieren
        meshRenderer = GetComponent<MeshRenderer>();

        // Eigene Materialinstanz erstellen, um geteilte Materialien nicht zu beeinflussen
        targetMaterial = new Material(meshRenderer.material);
        meshRenderer.material = targetMaterial;

        // Sicherstellen, dass das Material Transparenz unterstützt
        SetupMaterialForTransparency();

        initialPosition = transform.position;

        // Bounds des Spawn-Bereichs holen
        if (spawnArea != null)
        {
            Renderer areaRenderer = spawnArea.GetComponent<Renderer>();
            if (areaRenderer != null)
            {
                spawnBounds = areaRenderer.bounds;
            }
            else
            {
                Debug.LogError("SpawnArea hat keinen Renderer!");
            }
        }
        else
        {
            Debug.LogError("Kein SpawnArea zugewiesen!");
        }
    }

    void Update()
    {
        // Rotation
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Sinus-Wellenbewegung
        float newY = initialPosition.y + Mathf.Sin(Time.time * waveSpeed) * maxDisplacement;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        // Prüfen, ob das kollidierende Objekt das Tag "Missile" hat und kein Fade läuft
        if (other.CompareTag("Missile") && !isFading)
        {
            // Fade-Out starten, neue Position setzen und nach Verzögerung Fade-In
            StartCoroutine(Fade(1f, 0f, fadeDuration, () =>
            {
                MoveToNewPosition();
                Invoke(nameof(StartFadeIn), respawnDelay);
            }));
        }
    }

    void StartFadeIn()
    {
        // Fade-In starten
        StartCoroutine(Fade(0f, 1f, fadeDuration, null));
    }

    void SetupMaterialForTransparency()
    {
        // Material auf Transparent-Rendering einstellen
        targetMaterial.SetFloat("_Mode", 2); // Fade Mode
        targetMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        targetMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        targetMaterial.SetInt("_ZWrite", 0);
        targetMaterial.DisableKeyword("_ALPHATEST_ON");
        targetMaterial.EnableKeyword("_ALPHABLEND_ON");
        targetMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        targetMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    IEnumerator Fade(float startAlpha, float endAlpha, float duration, System.Action onComplete)
    {
        isFading = true;
        float elapsed = 0f;
        Color color = targetMaterial.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            targetMaterial.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // Sicherstellen, dass der Endwert erreicht wird
        targetMaterial.color = new Color(color.r, color.g, color.b, endAlpha);
        isFading = false;

        // Callback ausführen, falls vorhanden
        onComplete?.Invoke();
    }

    void MoveToNewPosition()
    {
        // Zufällige Position innerhalb der Bounds des Spawn-Bereichs
        Vector3 newPosition = new Vector3(
            Random.Range(spawnBounds.min.x, spawnBounds.max.x),
            Random.Range(spawnBounds.min.y, spawnBounds.max.y),
            Random.Range(spawnBounds.min.z, spawnBounds.max.z)
        );

        // Neue Position setzen und als Basis für die Wellenbewegung verwenden
        transform.position = newPosition;
        initialPosition = newPosition;
    }
}
