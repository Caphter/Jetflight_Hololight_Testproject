using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Waypoints : MonoBehaviour
{
    
    public List<Transform> wayPoints = new List<Transform>();
    public int target;
    public Transform objectToMove;
    private SipleCarScript carscript;
    //values for internal use
    private Quaternion lookRotation;
    private Vector3 direction;
    public bool loop = false;
    
    // Start is called before the first frame update
    void Start()
    {
        carscript = objectToMove.GetComponent<SipleCarScript>();
        objectToMove.position = wayPoints[0].position;
        target = 1;
       
        direction = (wayPoints[target].position - objectToMove.position);
        lookRotation = Quaternion.LookRotation(direction);
        Debug.Log("LookRotation " + direction);
        objectToMove.rotation = lookRotation;
        foreach(Transform t in wayPoints)
        {
            t.GetComponent<MeshRenderer>().enabled = false;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target >= 0)
        {
            if (Vector3.Distance(wayPoints[target].position, objectToMove.position) > 2f)
            {
                carscript.accelration = 1f;
                direction = (wayPoints[target].position - objectToMove.position);

                lookRotation = Quaternion.LookRotation(direction);
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                float currentAngle = objectToMove.eulerAngles.y;
                float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
                float steerInput = Mathf.Clamp((angleDifference / 180f) * 5f, -1f, 1f);
                //Debug.Log("currentAngle " + currentAngle+ "Object "+ (objectToMove.rotation.eulerAngles.y-lookRotation.eulerAngles.y) + "TargetAngle" + steerInput);
                carscript.stear = Mathf.Clamp(steerInput, -1f, 1f);
            }
            else
            {
                if (wayPoints.Count - 1 > target)
                {
                    target++;
                    Debug.Log("Next target");
                }
                else
                {
                    if (loop)
                    {
                        target = 0;
                    }
                    else
                    {
                        target = -1;
                        carscript.accelration = 0f;
                        Debug.Log("Stop driving");
                    }
                }
            }
        }
    }
}
