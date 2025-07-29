using UnityEngine;
using UnityEngine.InputSystem;


public class TargetingSystem : MonoBehaviour
{
    [SerializeField] private GameObject hmdCrosshairParent;
    [SerializeField] private GameObject triggerTargetingCrosshairParent; 

    [SerializeField] private InputActionReference triggerLeftPressedButton;
    [SerializeField] private InputActionReference primaryRightPressedButton;

    private bool triggerTargetingActive = false;
    private bool targetingSystemActive = false;


    private void Start()
    {
        triggerLeftPressedButton.action.Enable();
        triggerLeftPressedButton.action.performed += OnTriggerLeftPressed;
        triggerLeftPressedButton.action.canceled += OnTriggerLeftReleased;

        primaryRightPressedButton.action.Enable();
        primaryRightPressedButton.action.performed += OnPrimaryRightPressed;
    }

    private void OnDestroy()
    {
        if (triggerLeftPressedButton != null)
        {
            triggerLeftPressedButton.action.performed -= OnTriggerLeftPressed;
            triggerLeftPressedButton.action.canceled -= OnTriggerLeftReleased;

            primaryRightPressedButton.action.performed -= OnPrimaryRightPressed;
        }
    }

    private void OnTriggerLeftPressed(InputAction.CallbackContext context)
    {
        if(!targetingSystemActive)
        {
            triggerTargetingActive = true;
        }
    }

    private void OnTriggerLeftReleased(InputAction.CallbackContext context)
    {
        if (!targetingSystemActive)
        {
            triggerTargetingActive = false;
        }
    }

    private void OnPrimaryRightPressed(InputAction.CallbackContext context)
    {
        targetingSystemActive = !targetingSystemActive;
    }
}