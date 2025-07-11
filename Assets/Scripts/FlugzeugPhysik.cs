using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlugzeugPhysik : MonoBehaviour
{
    // Flugzeuggeschwindigkeits-Variablen
    public float schubkraft = 500f; // Schubkraft
    public float rollGeschwindigkeit = 50f; // Rollgeschwindigkeit
    public float pitchGeschwindigkeit = 25f; // Pitch-Geschwindigkeit
    public float yawGeschwindigkeit = 100f; // Yaw-Geschwindigkeit

    // Weitere Variablen für das Flugzeug
    public float maxNeigungswinkel = 30f; // Maximaler Neigungswinkel
    public float minNeigungswinkel = -30f; // Minimaler Neigungswinkel

    private Rigidbody rb;

    // Steuerungseingaben
    private float pitch;
    private float roll;
    private float yaw;
    private float schub;

    void Start()
    {
        // Rigidbody-Komponente holen
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Wir steuern die Schwerkraft hier selbst
    }

    void Update()
    {
        // Steuerungseingaben
        pitch = Input.GetAxis("Vertical");  // Auf/Ab (W/S, Pfeiltasten)
        roll = Input.GetAxis("Horizontal"); // Links/Rechts (A/D, Pfeiltasten)
        yaw = Input.GetAxis("Yaw");         // Gier (Q/E)

        // Flugzeugsteuerung (Schubkraft)
        schub = Input.GetAxis("Thrust") * schubkraft; // W und S für Schub

        // Flugzeugbewegungen anwenden
        FlugzeugSteuerung();
    }

    void FlugzeugSteuerung()
    {
        // Schub anwenden (Vorwärtsbewegung)
        rb.AddForce(transform.forward * schub, ForceMode.Force);

        // Drehung um die Achsen (Roll, Pitch, Yaw)
        transform.Rotate(Vector3.forward, -roll * rollGeschwindigkeit * Time.deltaTime); // Roll
        transform.Rotate(Vector3.right, pitch * pitchGeschwindigkeit * Time.deltaTime);   // Pitch
        transform.Rotate(Vector3.up, yaw * yawGeschwindigkeit * Time.deltaTime);         // Yaw

        // Begrenzen der Neigungswinkel für Pitch (Höhensteuerung)
        float currentPitch = transform.eulerAngles.x;
        if (currentPitch > 180) currentPitch -= 360; // Korrigiert den Winkelbereich
        currentPitch = Mathf.Clamp(currentPitch, minNeigungswinkel, maxNeigungswinkel);
        transform.eulerAngles = new Vector3(currentPitch, transform.eulerAngles.y, transform.eulerAngles.z);
    }
}
