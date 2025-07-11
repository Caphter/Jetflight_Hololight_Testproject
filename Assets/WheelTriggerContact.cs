using UnityEngine;

public class WheelTriggerContact : MonoBehaviour
{
    public bool isTouchingGround = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            isTouchingGround = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            isTouchingGround = false;
        }
    }
}