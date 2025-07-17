using UnityEngine;
using UnityEngine.SceneManagement; // Wichtig: Für das Neuladen von Szenen
using System.Collections; // Für Coroutinen

public class PlaneTerrainCollisionCheck : MonoBehaviour
{
    [Header("Screen Fade Settings")]
    [SerializeField] private CanvasGroup fadePanelCanvasGroup; // Zuweisen im Inspector: Ein UI-Panel mit CanvasGroup
    [SerializeField] private float fadeDuration = 1.0f; // Dauer des Abdunkelns in Sekunden

    private bool isFading = false; // Verhindert mehrfaches Auslösen des Fades

    // OnTriggerEnter wird aufgerufen, wenn dieser Collider (als Trigger) einen anderen Collider berührt
    private void OnTriggerEnter(Collider other)
    {
        // Überprüfen, ob das Objekt mit dem Tag "Terrain" kollidiert ist UND der Fade-Vorgang noch nicht läuft
        if (other.CompareTag("Terrain") && !isFading)
        {
            isFading = true; // Setze Flag, um erneutes Auslösen zu verhindern
            Debug.Log("Kollision mit Terrain! Bildschirm wird abgedunkelt und Szene neu geladen.");
            StartCoroutine(FadeOutAndRestartScene());
        }
    }

    private IEnumerator FadeOutAndRestartScene()
    {
        // Stelle sicher, dass das Panel am Anfang komplett transparent ist und Interaktionen blockiert
        fadePanelCanvasGroup.alpha = 0f;
        fadePanelCanvasGroup.blocksRaycasts = true; // Blockiert Eingaben, während gefaded wird

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // Lineares Überblenden der Alpha-Werte von 0 (transparent) auf 1 (undurchsichtig)
            fadePanelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null; // Warte einen Frame
        }
        fadePanelCanvasGroup.alpha = 1f; // Sicherstellen, dass es komplett undurchsichtig ist

        // Warte einen kurzen Moment, nachdem der Bildschirm schwarz ist (optional)
        yield return new WaitForSeconds(0.5f);

        // Lade die aktuelle Szene neu
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}