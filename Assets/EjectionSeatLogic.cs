using UnityEngine;
using System.Collections;

public class EjectionSeatLogic : MonoBehaviour
{
    [Header("Ejection Handle")]
    [SerializeField] private GameObject ejectionSeatHandle;
    private Vector3 ejectionSeatHandleStartLocalPositionRelativeToPlane;
    private Quaternion ejectionSeatHandleStartLocalRotationRelativeToPlane;
    [SerializeField] private float ejectionHandleDistanceThreshold = 0.3f;
    [SerializeField] private float handleDeactivationDelay = 0.5f;
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
    [SerializeField] private GameObject parachuteObject;
    [SerializeField] private Vector3 parachuteStartScale;
    [SerializeField] private Vector3 parachutTargetScale;
    [SerializeField] private float parachuteDeployDuration = 1f;
    [SerializeField] private float parachuteDrag = 0.5f;
    [SerializeField] private float parachuteDelay = 2f;

    [Header("Vignette During Ejection")] // Changed header to reflect only ejection
    [SerializeField] private MotionSicknessVignetteLogic motionSicknessVignetteLogic;
    [Tooltip("Aperture size for the strong ejection vignette (e.g., 0.1 for very strong).")]
    [Range(0f, 1f)][SerializeField] private float ejectionVignetteAperture = 0.1f;
    [Tooltip("Ease-in time for the strong ejection vignette.")]
    [SerializeField] private float ejectionVignetteEaseInTime = 0.2f;
    [Tooltip("Duration for the strong ejection vignette to last.")]
    [SerializeField] private float ejectionVignetteDuration = 2.0f;


    [Header("References")]
    [SerializeField] private AirplaneMovementController airplaneMovementController;

    public bool ejectionSequenceStarted = false;

    private Coroutine returnHandleCoroutine;

    private void Start()
    {
        ejectionSeatHandleStartLocalPositionRelativeToPlane = ejectionSeatHandle.transform.localPosition;
        ejectionSeatHandleStartLocalRotationRelativeToPlane = ejectionSeatHandle.transform.localRotation;
        parachuteStartScale = parachuteObject.transform.localScale;
    }

    private void Update()
    {
        float currentDistance;

        if (ejectionSeatHandle.transform.parent == this.transform)
        {
            currentDistance = Vector3.Distance(ejectionSeatHandleStartLocalPositionRelativeToPlane, ejectionSeatHandle.transform.localPosition);
        }
        else
        {
            Vector3 handleWorldPosition = ejectionSeatHandle.transform.position;
            Vector3 handleLocalPositionInPlaneSpace = this.transform.InverseTransformPoint(handleWorldPosition);

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
        if (returnHandleCoroutine != null)
        {
            StopCoroutine(returnHandleCoroutine);
        }

        ejectionSeatHandle.SetActive(false);

        EjectCockpitCover();

        // Trigger strong ejection vignette for a duration, then it will fade out completely
        if (motionSicknessVignetteLogic != null)
        {
            motionSicknessVignetteLogic.RequestTemporaryVignette(
                ejectionVignetteAperture,
                ejectionVignetteEaseInTime,
                ejectionVignetteDuration
            );
        }


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
        if (seatRb != null)
        {
            seatRb.drag = parachuteDrag;
            seatRb.angularDrag = parachuteDrag;
        }

        StartCoroutine(ScaleParachuteSmoothly());

        Invoke(nameof(ResetScene), 20f);
    }

    private IEnumerator ScaleParachuteSmoothly()
    {
        float timer = 0f;
        Vector3 currentScale = parachuteObject.transform.localScale;

        while (timer < parachuteDeployDuration)
        {
            timer += Time.deltaTime;
            parachuteObject.transform.localScale = Vector3.Lerp(currentScale, parachutTargetScale, timer / parachuteDeployDuration);
            yield return null;
        }

        parachuteObject.transform.localScale = parachutTargetScale;
    }

    public void ResetScene()
    {
        if (motionSicknessVignetteLogic != null)
        {
            motionSicknessVignetteLogic.ReleaseVignetteControl();
        }
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void HandleReleased()
    {
        if (!ejectionSequenceStarted)
        {
            ejectionSeatHandle.transform.SetParent(this.transform, true);
            if (returnHandleCoroutine != null)
            {
                StopCoroutine(returnHandleCoroutine);
            }
            returnHandleCoroutine = StartCoroutine(ReturnHandleSmoothly());
        }
    }

    private IEnumerator ReturnHandleSmoothly()
    {
        float t = 0f;
        Vector3 startPos = ejectionSeatHandle.transform.localPosition;
        Quaternion startRot = ejectionSeatHandle.transform.localRotation;

        while (t < 1f)
        {
            t += Time.deltaTime * handleReturnSpeed;
            ejectionSeatHandle.transform.localPosition = Vector3.Lerp(startPos, ejectionSeatHandleStartLocalPositionRelativeToPlane, t);
            ejectionSeatHandle.transform.localRotation = Quaternion.Lerp(startRot, ejectionSeatHandleStartLocalRotationRelativeToPlane, t);
            yield return null;
        }

        ejectionSeatHandle.transform.localPosition = ejectionSeatHandleStartLocalPositionRelativeToPlane;
        ejectionSeatHandle.transform.localRotation = ejectionSeatHandleStartLocalRotationRelativeToPlane;
        returnHandleCoroutine = null;
    }
}