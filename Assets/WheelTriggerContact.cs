using UnityEngine;

public class WheelTriggerContact : MonoBehaviour
{
    public bool isTouchingGround = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ground"))
        {
            isTouchingGround = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ground"))
        {
            isTouchingGround = false;
        }
    }
}