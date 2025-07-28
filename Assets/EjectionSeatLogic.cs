using UnityEngine;
using System.Collections;

public class EjectionSeatLogic : MonoBehaviour
{
    [Header("Ejection Handle")]
    [SerializeField] private GameObject ejectionSeatHandle;
    private Vector3 ejectionSeatHandleStartLocalPositionRelativeToPlane; // Speichert die lokale Position relativ zum Flugzeug
    [SerializeField] private float ejectionHandleDistanceThreshold = 0.3f;
    [SerializeField] private float handleReturnSpeed = 5f;


    [Header("Cockpit Cover")]
    [SerializeField] private GameObject cockpitCover;
    [SerializeField] private float coverEjectionForce = 500f;
    [SerializeField] private float coverEjectionTorque = 100f;

    [Header("Ejection Seat")]
    [SerializeField] private GameObject pilotSeatObject;
    [SerializeField] private float seatEjectionForce = 1000f;
    [SerializeField] private float seatInitialDrag = 0.1f;
    [SerializeField] private GameObject xrRig;

    [Header("Parachute")]
    [SerializeField] private float parachuteDrag = 0.5f;
    [SerializeField] private float parachuteDelay = 2f;

    [Header("References")]
    [SerializeField] private AirplaneMovementController airplaneMovementController;

    public bool ejectionSequenceStarted = false;
    private Coroutine returnHandleCoroutine; // To manage the smooth return


    private void Start()
    {
        ejectionSeatHandleStartLocalPositionRelativeToPlane = ejectionSeatHandle.transform.localPosition;
    }

    private void Update()
    {
        float currentDistance;

        if (ejectionSeatHandle.transform.parent == this.transform) // Annahme: Dieses Skript ist am Flugzeug, und der Griff ist direktes Kind
        {
            currentDistance = Vector3.Distance(ejectionSeatHandleStartLocalPositionRelativeToPlane, ejectionSeatHandle.transform.localPosition);
        }
        else // Der Griff wurde entparented (z.B. von der VR-Hand gegriffen)
        {
            // Konvertiere die aktuelle Position des Griffs in den lokalen Raum des Flugzeugs
            Vector3 handleWorldPosition = ejectionSeatHandle.transform.position;
            Vector3 handleLocalPositionInPlaneSpace = this.transform.InverseTransformPoint(handleWorldPosition);

            // Vergleiche diese konvertierte Position mit der gespeicherten Startposition
            currentDistance = Vector3.Distance(ejectionSeatHandleStartLocalPositionRelativeToPlane, handleLocalPositionInPlaneSpace);
        }

        Debug.Log("Current Distance: " + currentDistance);

        if (currentDistance > ejectionHandleDistanceThreshold && !ejectionSequenceStarted)
        {
            ejectionSequenceStarted = true;
            StartEjectionSequence();
        }
    }

    private void StartEjectionSequence()
    {
        ejectionSeatHandle.SetActive(false);

        EjectCockpitCover();


        Invoke(nameof(EjectSeat), 0.2f);
        Invoke(nameof(DeployParachute), parachuteDelay);
    }

    public void EjectCockpitCover()
    {
        Rigidbody coverRb = cockpitCover.GetComponent<Rigidbody>();

        cockpitCover.transform.SetParent(null, true);

        coverRb.useGravity = true;
        coverRb.isKinematic = false;

        Vector3 ejectionDirection = cockpitCover.transform.up;
        coverRb.AddForce(ejectionDirection * coverEjectionForce, ForceMode.Impulse);

        coverRb.AddTorque(Random.insideUnitSphere * coverEjectionTorque, ForceMode.Impulse);

        Destroy(cockpitCover, 10f);
    }

    public void EjectSeat()
    {
        xrRig.transform.SetParent(pilotSeatObject.transform, true);

        Rigidbody seatRb = pilotSeatObject.GetComponent<Rigidbody>();

        pilotSeatObject.transform.SetParent(null, true);

        seatRb.useGravity = true;
        seatRb.isKinematic = false;
        seatRb.drag = seatInitialDrag;

        seatRb.AddForce(transform.up * seatEjectionForce, ForceMode.Impulse);
    }

    public void DeployParachute()
    {
        Rigidbody seatRb = pilotSeatObject.GetComponent<Rigidbody>();
        if (seatRb != null) // Dieses if (null) behalte ich bei, da es sich um eine Referenz handelt, die zur Laufzeit null werden könnte, was zu einem Fehler führen würde
        {
            seatRb.drag = parachuteDrag;
            seatRb.angularDrag = parachuteDrag;
        }

        Invoke(nameof(ResetScene), 5f);
    }

    public void ResetScene()
    {
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void HandleReleased()
    {
        if (!ejectionSequenceStarted)
        {
            returnHandleCoroutine = StartCoroutine(ReturnHandleSmoothly());
        }
    }

    private IEnumerator ReturnHandleSmoothly()
    {
        while (Vector3.Distance(ejectionSeatHandle.transform.localPosition, ejectionSeatHandleStartLocalPositionRelativeToPlane) > 0.001f)
        {
            ejectionSeatHandle.transform.localPosition = Vector3.Lerp(
                ejectionSeatHandle.transform.localPosition,
                ejectionSeatHandleStartLocalPositionRelativeToPlane,
                Time.deltaTime * handleReturnSpeed
            );
            yield return null;
        }
        // Ensure it snaps exactly to the start position at the end
        ejectionSeatHandle.transform.localPosition = ejectionSeatHandleStartLocalPositionRelativeToPlane;
        returnHandleCoroutine = null;
    }
}