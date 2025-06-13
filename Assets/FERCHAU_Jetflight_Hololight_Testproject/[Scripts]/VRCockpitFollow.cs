using UnityEngine;

public class VRCockpitFollow : MonoBehaviour
{
    public Transform jet;               // Der Jet
    public Transform cockpitAnchor;     // Ein leeres Transform-Objekt im Jet (z. B. Sitzposition)
    public float positionSmooth = 5f;
    public float rotationSmooth = 2f;

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, cockpitAnchor.position, Time.deltaTime * positionSmooth);
        transform.rotation = Quaternion.Slerp(transform.rotation, cockpitAnchor.rotation, Time.deltaTime * rotationSmooth);
    }
}