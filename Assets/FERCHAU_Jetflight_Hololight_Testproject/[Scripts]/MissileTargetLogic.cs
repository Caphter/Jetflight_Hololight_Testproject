using UnityEngine;
using System.Collections;

public class MissileTargetLogic : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 30f;

    private Vector3 initialPosition;


    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
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