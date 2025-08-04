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

    private Quaternion airplaneEjectRotationYOnly;

    [Header("Parachute")]
    [SerializeField] private GameObject parachuteObject;
    [SerializeField] private Vector3 parachuteStartScale;
    [SerializeField] private Vector3 parachutTargetScale;
    [SerializeField] private float parachuteDeployDuration = 1f;
    [SerializeField] private float parachuteDrag = 0.5f;
    [SerializeField] private float parachuteDelay = 2f; // Das ist immer noch die Verzögerung FÜR DEN FALLSCHIRM selbst

    [Header("Parachute Descent Physics")]
    [Tooltip("Geschwindigkeit, mit der sich der Sitz nach dem Fallschirm-Deploy aufrichtet.")]
    [SerializeField] private float alignSpeed = 2f;
    [Tooltip("Stärke des Schwingeffekts.")]
    [SerializeField] private float swingMagnitude = 5f;
    [Tooltip("Frequenz des Schwingeffekts.")]
    [SerializeField] private float swingFrequency = 1f;
    [Tooltip("Dämpfung des Schwingeffekts, damit er nicht endlos schwingt.")]
    [SerializeField] private float swingDamping = 0.5f;
    [Tooltip("Verzögerungszeit nach dem Sitz-Auswurf, bis der Sitz beginnt, sich aufzurichten und zu schwingen.")]
    [SerializeField] private float alignDelay = 1.0f; // NEU: Unabhängige Verzögerung für die Ausrichtung


    [Header("Vignette During Ejection")]
    [SerializeField] private MotionSicknessVignetteLogic motionSicknessVignetteLogic;
    [Tooltip("Aperture size for the strong ejection vignette (e.g., 0.1 for very strong).")]
    [Range(0f, 1f)][SerializeField] private float ejectionVignetteAperture = 0.1f;
    [Tooltip("Ease-in time for the strong ejection vignette.")]
    [SerializeField] private float ejectionVignetteEaseInTime = 0.2f;
    [Tooltip("Duration for the strong ejection vignette to last.")]
    [SerializeField] private float ejectionVignetteDuration = 2.0f;


    [Header("Audio Settings")]
    [SerializeField] private AudioSource engineAudioSource;
    [SerializeField] private float volumeBoostFactor = 1.5f;
    private float originalEngineVolume;


    [Header("References")]
    [SerializeField] private AirplaneMovementController airplaneMovementController;

    public bool ejectionSequenceStarted = false;

    private Coroutine returnHandleCoroutine;

    private void Start()
    {
        ejectionSeatHandleStartLocalPositionRelativeToPlane = ejectionSeatHandle.transform.localPosition;
        ejectionSeatHandleStartLocalRotationRelativeToPlane = ejectionSeatHandle.transform.localRotation;
        parachuteStartScale = parachuteObject.transform.localScale;

        // NEU: Ursprüngliche Lautstärke der AudioSource speichern
        if (engineAudioSource != null)
        {
            originalEngineVolume = engineAudioSource.volume;
        }
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

        // NEU: Lautstärke des Triebwerkssounds erhöhen
        if (engineAudioSource != null)
        {
            engineAudioSource.volume = originalEngineVolume * volumeBoostFactor;
        }

        if (motionSicknessVignetteLogic != null)
        {
            motionSicknessVignetteLogic.RequestTemporaryVignette(
                ejectionVignetteAperture,
                ejectionVignetteEaseInTime,
                ejectionVignetteDuration
            );
        }

        if (airplaneMovementController != null)
        {
            Vector3 planeForward = airplaneMovementController.transform.forward;
            airplaneEjectRotationYOnly = Quaternion.Euler(0, Quaternion.LookRotation(new Vector3(planeForward.x, 0, planeForward.z)).eulerAngles.y, 0);
        }
        else
        {
            airplaneEjectRotationYOnly = Quaternion.identity;
        }

        Invoke(nameof(EjectSeat), 0.2f);
        // NEU: Ausrichtung des Sitzes beginnt nach alignDelay (vom Eject-Moment an)
        Invoke(nameof(StartAlignAndSwing), alignDelay);

        // NEU: Setze Angular Drag für das Schwingen hier, damit es zum Zeitpunkt der Ausrichtung wirksam wird
        Invoke(nameof(ApplyParachuteDrag), 0.5f);

        // Fallschirm-Deploy bleibt bei parachuteDelay
        Invoke(nameof(DeployParachute), parachuteDelay);
    }

    public void EjectCockpitCover()
    {
        FindObjectOfType<AudioManager>().Play("Cockpit_Cover_Eject");

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
        FindObjectOfType<AudioManager>().Play("Seat_Rockets");

        xrRig.transform.SetParent(pilotSeatObject.transform, true);

        Rigidbody seatRb = pilotSeatObject.GetComponent<Rigidbody>();

        pilotSeatObject.transform.SetParent(null, true);

        seatRb.useGravity = true;
        seatRb.isKinematic = false;
        seatRb.drag = seatInitialDrag;

        seatRb.AddForce(transform.up * seatEjectionForce, ForceMode.Impulse);
    }

    // NEU: Hilfsfunktion, die von Invoke aufgerufen wird, um die Coroutine zu starten
    private void StartAlignAndSwing()
    {
        Rigidbody seatRb = pilotSeatObject.GetComponent<Rigidbody>();
        if (seatRb != null)
        {
            // Setze Angular Drag für das Schwingen hier zurück, damit es zum Zeitpunkt der Ausrichtung wirksam wird
            seatRb.angularDrag = parachuteDrag;
            seatRb.freezeRotation = false;
            StartCoroutine(AlignAndSwingSeat(seatRb));
        }
    }

    void ApplyParachuteDrag(Rigidbody seatRb)
    {
        if (seatRb != null)
        {
            seatRb.drag = parachuteDrag;
        }
    }

    public void DeployParachute()
    {
        StartCoroutine(ScaleParachuteSmoothly());

        Invoke(nameof(ResetScene), 20f);
    }

    private IEnumerator ScaleParachuteSmoothly()
    {
        FindObjectOfType<AudioManager>().Play("Parachute_Open");
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

    private IEnumerator AlignAndSwingSeat(Rigidbody seatRb)
    {
        // Phase 1: Sanft aufrichten
        float alignTimer = 0f;
        Quaternion initialCurrentRotation = pilotSeatObject.transform.rotation;

        Quaternion targetRotation = Quaternion.LookRotation(airplaneEjectRotationYOnly * Vector3.forward, Vector3.up);

        Debug.DrawRay(pilotSeatObject.transform.position, targetRotation * Vector3.forward * 5, Color.blue, 10f);
        Debug.DrawRay(pilotSeatObject.transform.position, targetRotation * Vector3.up * 5, Color.green, 10f);


        while (alignTimer < 1f)
        {
            alignTimer += Time.deltaTime * alignSpeed;
            pilotSeatObject.transform.rotation = Quaternion.Slerp(initialCurrentRotation, targetRotation, alignTimer);
            yield return null;
        }
        pilotSeatObject.transform.rotation = targetRotation;

        // Phase 2: Leichtes Schwingen mit Dämpfung
        float swingTimer = 0f;

        while (true)
        {
            swingTimer += Time.deltaTime * swingFrequency;

            float currentDamping = Mathf.Exp(-swingDamping * swingTimer);
            if (currentDamping < 0.01f && swingTimer > 5f)
            {
                pilotSeatObject.transform.rotation = targetRotation;
                break;
            }

            float swingPitch = Mathf.Sin(swingTimer) * swingMagnitude * currentDamping;
            float swingRoll = Mathf.Cos(swingTimer * 0.7f) * swingMagnitude * 0.5f * currentDamping;

            Quaternion swingOffset = Quaternion.Euler(swingPitch, 0f, swingRoll);

            Quaternion combinedSwingRotation = targetRotation * swingOffset;

            pilotSeatObject.transform.rotation = Quaternion.Slerp(pilotSeatObject.transform.rotation, combinedSwingRotation, Time.deltaTime * alignSpeed);

            yield return null;
        }
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