using UnityEngine;
using System.Collections;

public class MissileTargetLogic : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 30f;

    [Header("Wave Movement Settings")]
    [SerializeField] private float waveSpeed = 1f;
    [SerializeField] private float maxDisplacement = 0.5f;

    private Vector3 initialPosition;


    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        float newY = initialPosition.y + Mathf.Sin(Time.time * waveSpeed) * maxDisplacement;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Missile"))
        {
            FindObjectOfType<AudioManager>().Play("Target_Hit");
            gameObject.SetActive(false);
        }
    }
}