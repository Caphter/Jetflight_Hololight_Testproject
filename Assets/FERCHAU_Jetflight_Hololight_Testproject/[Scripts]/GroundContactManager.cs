using UnityEngine;
using System.Collections;

public class GroundContactManager : MonoBehaviour
{
    public int currentGroundedWheels = 0;
    public bool isGrounded = true;

    [Header("Wheel Touching Scripts")]
    [SerializeField] private WheelTriggerContact frontWheelContact;
    [SerializeField] private WheelTriggerContact rearLeftWheelContact;
    [SerializeField] private WheelTriggerContact rearRightWheelContact;

    [Header("Take-off/Landing Settings")]
    [SerializeField] private float minAirTimeForTakeOff = 3.0f;

    private bool hasTakenOffAfterLanding = false;
    private float timeNotInAir = 0f;

    void FixedUpdate()
    {
        CheckGroundContact();
    }

    private void CheckGroundContact()
    {
        currentGroundedWheels = 0;

        if (frontWheelContact.isTouchingGround)
        {
            currentGroundedWheels++;
        }

        if (rearLeftWheelContact.isTouchingGround)
        {
            currentGroundedWheels++;
        }

        if (rearRightWheelContact.isTouchingGround)
        {
            currentGroundedWheels++;
        }

        if (currentGroundedWheels >= 1)
        {
            isGrounded = true;
            timeNotInAir = 0f;

            if (hasTakenOffAfterLanding)
            {
                FindObjectOfType<AudioManager>().Play("Jet_Landing");
                hasTakenOffAfterLanding = false;
            }
        }
        else
        {
            isGrounded = false;
            timeNotInAir += Time.fixedDeltaTime;

            if (timeNotInAir >= minAirTimeForTakeOff)
            {
                hasTakenOffAfterLanding = true;
            }
        }
    }
}