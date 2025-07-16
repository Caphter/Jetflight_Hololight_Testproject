using UnityEngine;


public class GroundContactManager : MonoBehaviour
{
    public int currentGroundedWheels = 0;
    public bool isGrounded = true;

    [Header("Wheel Touching Scripts")]
    [SerializeField] private WheelTriggerContact frontWheelContact;
    [SerializeField] private WheelTriggerContact rearLeftWheelContact;
    [SerializeField] private WheelTriggerContact rearRightWheelContact;


    void FixedUpdate()
    {
        CheckGroundContact();
    }

    private void CheckGroundContact()
    {
        currentGroundedWheels = 0;

        if(frontWheelContact.isTouchingGround)
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

        if(currentGroundedWheels == 3)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
}