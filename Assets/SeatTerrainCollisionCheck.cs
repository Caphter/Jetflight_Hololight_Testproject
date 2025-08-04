using UnityEngine;

public class SeatTerrainCollisionCheck : MonoBehaviour
{
    [SerializeField] private ScreenFaderVR screenFaderScript;
    [SerializeField] private Rigidbody seatRigidbody; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Terrain")
        {
            FindObjectOfType<AudioManager>().Play("Jet_Landing");

            if (seatRigidbody != null)
            {
                seatRigidbody.velocity = Vector3.zero;        
                seatRigidbody.angularVelocity = Vector3.zero;
                seatRigidbody.isKinematic = true;             
            }

            StartCoroutine(screenFaderScript.FadeToBlack(1f, true));
        }
    }
}