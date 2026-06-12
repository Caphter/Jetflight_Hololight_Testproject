using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

public class SetRigToReferencePoint : MonoBehaviour
{
    [SerializeField] private XROrigin xrRig; // XR Rig (XROrigin)
    [SerializeField] private Transform referencePoint; // Referenzpunkt im Cockpit
    [SerializeField] private InputActionReference rightSecondaryButton;

    private void Start()
    {
        if (rightSecondaryButton != null && rightSecondaryButton.action != null)
        {
            rightSecondaryButton.action.performed += OnSecondaryButtonPressed;
        }
        else
        {
            Debug.LogWarning("rightSecondaryButton oder dessen Aktion ist nicht zugewiesen!");
        }

        // Verzögerter Aufruf, um sicherzustellen, dass das XR-System initialisiert ist
        Invoke("SetRigToReference", 0.5f);
    }

    private void OnDestroy()
    {
        if (rightSecondaryButton != null && rightSecondaryButton.action != null)
        {
            rightSecondaryButton.action.performed -= OnSecondaryButtonPressed; // Cleanup
        }
    }

    private void OnSecondaryButtonPressed(InputAction.CallbackContext context)
    {
        Debug.Log("Secondary Button Pressed");
        SetRigToReference();
    }

    public void SetRigToReference()
    {
        // Hole die Kamera (Headset) des XR Rigs
        Camera mainCamera = xrRig.GetComponentInChildren<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("Keine Kamera im XR Rig gefunden!");
            return;
        }

        // Berechne den lokalen Offset der Kamera relativ zum XR Rig
        Vector3 cameraLocalOffset = mainCamera.transform.localPosition;

        // Berechne die Zielposition des XR Rigs, sodass die Kamera genau am Referenzpunkt liegt
        Vector3 targetPosition = referencePoint.position - mainCamera.transform.parent.TransformVector(cameraLocalOffset);

        // Setze Position und Rotation des XR Rigs
        xrRig.transform.position = targetPosition;
        xrRig.transform.rotation = referencePoint.rotation;
    }
}