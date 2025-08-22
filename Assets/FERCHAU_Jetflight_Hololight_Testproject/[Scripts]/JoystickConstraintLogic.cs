using UnityEngine;
using System.Collections;

public class JoystickConstraintLogic : MonoBehaviour
{
    [SerializeField] private GameObject joystickObject;
    [SerializeField] private GameObject joystickObjectInvisible;
    [SerializeField] private Transform originJoystickTransform;
    [SerializeField] private GameObject rightControllerMesh;
    [SerializeField] private float returnLerpSpeed = 5f;

    private bool joystickGrabbed = false;
    private Coroutine returnToCenterRoutine;
    private Quaternion joystickInitialRotation;

    [SerializeField] private float maxXRotation = 30f;
    [SerializeField] private float maxZRotation = 30f;
    [SerializeField] private float xRotationScaling = 2f;
    [SerializeField] private float zRotationScaling = 2f;

    public float NormalizedPitch { get; private set; }
    public float NormalizedRoll { get; private set; }

    private void Start()
    {
        joystickInitialRotation = joystickObject.transform.localRotation;
    }

    private void Update()
    {
        if (joystickGrabbed)
        {
            CalculateJoystickRotation();
        }
        else
        {
            joystickObjectInvisible.transform.position = originJoystickTransform.position;
            joystickObjectInvisible.transform.rotation = originJoystickTransform.rotation;
        }
    }

    private void CalculateJoystickRotation()
    {
        Vector3 localPositionDelta = originJoystickTransform.InverseTransformDirection(joystickObjectInvisible.transform.position - originJoystickTransform.position);

        float xRotation = localPositionDelta.z * maxXRotation * xRotationScaling;
        float zRotation = -localPositionDelta.x * maxZRotation * zRotationScaling;

        float rotationMagnitude = Mathf.Sqrt(xRotation * xRotation + zRotation * zRotation);

        if (rotationMagnitude > maxXRotation)
        {
            float scale = maxXRotation / rotationMagnitude;
            xRotation *= scale;
            zRotation *= scale;
        }

        joystickObject.transform.localRotation = Quaternion.Euler(xRotation, 0f, zRotation);

        NormalizedPitch = xRotation / maxXRotation;
        NormalizedRoll = zRotation / maxZRotation;
    }

    public void GrabButtonPressed()
    {
        joystickGrabbed = true;
        rightControllerMesh.SetActive(false);

        if (returnToCenterRoutine != null)
        {
            StopCoroutine(returnToCenterRoutine);
        }
    }

    public void GrabButtonReleased()
    {
        joystickGrabbed = false;
        rightControllerMesh.SetActive(true);
        returnToCenterRoutine = StartCoroutine(ReturnToCenter());
    }

    private IEnumerator ReturnToCenter()
    {
        while (!joystickGrabbed && Quaternion.Angle(joystickObject.transform.localRotation, joystickInitialRotation) > 0.1f)
        {
            joystickObject.transform.localRotation = Quaternion.Lerp(joystickObject.transform.localRotation, joystickInitialRotation, returnLerpSpeed * Time.deltaTime);
            yield return null;
        }

        joystickObject.transform.localRotation = joystickInitialRotation;
        NormalizedPitch = 0f;
        NormalizedRoll = 0f;
        returnToCenterRoutine = null;
    }
}