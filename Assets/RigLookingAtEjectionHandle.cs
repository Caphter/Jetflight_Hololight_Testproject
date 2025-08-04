using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RigLookingAtEjectionHandle : MonoBehaviour
{
    private bool overwriteBecauseGrabbed = false;

    public bool handleIsBeingLookedAt = false;

    [SerializeField] private GameObject handleObject;
    [SerializeField] private GameObject hoverObject;
    [SerializeField] private Material hoiverHandleMaterial;
    [SerializeField] private Color noHover = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private XRGrabInteractable grabInteractableComponent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "EjectionHandle")
        {
            handleIsBeingLookedAt = true;
            grabInteractableComponent.enabled = true;

            if(!overwriteBecauseGrabbed)
            {
                hoverObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!overwriteBecauseGrabbed && other.tag == "EjectionHandle")
        {
            handleIsBeingLookedAt = false;
            grabInteractableComponent.enabled = false;
            hoverObject.SetActive(false);
        }
    }


    public void HandleGrabbed()
    {
        overwriteBecauseGrabbed = true;
        handleIsBeingLookedAt = true;
        hoverObject.SetActive(false);
    }

    public void HandleReleased()
    {
        overwriteBecauseGrabbed = false;
    }

    public void OnHoverEnter()
    {
        if (hoiverHandleMaterial != null)
        {
            hoiverHandleMaterial.color = hoverColor;
        }
    }

    public void OnHoverExit()
    {
        if (hoiverHandleMaterial != null)
        {
            hoiverHandleMaterial.color = noHover;
        }
    }
}
