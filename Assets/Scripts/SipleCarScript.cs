using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SipleCarScript : MonoBehaviour
{
    [Header("Max Speed = m/s, Max stear = angle")]
    public float speedMax = 1.6f;
    public float stearMax = 35.0f;
    public float wheelRadius = 1f;

    [Header("external values")]
    public float accelration;
    public float stear;

    private float accelrationValue;
    private float stearValue;

    [Header("how smooth is the animation")]
    public float smoothAccelrate = 0.5f;
    public float smoothStear = 0.5f;
    
    [Header("animatet parts")]
    public GameObject wheel_FL_OfsGp;
    public GameObject wheel_FL;
    public GameObject wheel_FR_OfsGp;
    public GameObject wheel_FR;
    public GameObject wheel_BL;
    public GameObject wheel_BR;
    public bool moreAsFourWheels = false;
    public GameObject wheel_ML;
    public GameObject wheel_MR;

    [Header("animatet body")]
    public GameObject carBody;

    private float accelVelocity;
    private float stearlVelocity;
    private float wheelRotation;

    public bool canPlayAniamtions = false;
    public float animspeed = 1f;
    private Vector3 oldPos;
    private float randomeValue;
    

    // Start is called before the first frame update
    void Start()
    {
        randomeValue = Random.Range(0.8f, 1.2f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
            accelration = 1.0f;
        if (Input.GetKeyUp(KeyCode.W))
            accelration = 0.0f;

        if (Input.GetKey(KeyCode.S))
            accelration = -1.0f;
        if (Input.GetKeyUp(KeyCode.S))
            accelration = 0.0f;

        if (Input.GetKey(KeyCode.D))
            stear = 1.0f;
        if (Input.GetKeyUp(KeyCode.D))
            stear = 0.0f;

        if (Input.GetKey(KeyCode.A))
            stear = -1.0f;
        if (Input.GetKeyUp(KeyCode.A))
            stear = 0.0f;


        accelrationValue = Mathf.SmoothDamp(accelrationValue, speedMax * accelration, ref accelVelocity, smoothAccelrate);
        transform.Translate(0f, 0f, accelrationValue * Time.deltaTime);

        stearValue = Mathf.SmoothDamp(stearValue, stearMax * stear, ref stearlVelocity, smoothStear);
        wheel_FL_OfsGp.transform.localEulerAngles = new Vector3(0f, stearValue, 0f);
        wheel_FR_OfsGp.transform.localEulerAngles = new Vector3(0f, stearValue, 0f);


        float rotate = (1f / stearMax) * stearValue;
        float accel = (1f / speedMax) * accelrationValue;
        transform.localEulerAngles += new Vector3(0f, stearValue * accel * Time.deltaTime, 0f);

        wheelRotation = accelrationValue * (wheelRadius * 3.14159f);
        wheel_FL.transform.Rotate(wheelRotation, 0f, 0f);
        wheel_FR.transform.Rotate(wheelRotation, 0f, 0f);
        wheel_BL.transform.Rotate(wheelRotation, 0f, 0f);
        wheel_BR.transform.Rotate(wheelRotation, 0f, 0f);
        if(moreAsFourWheels)
        {
            wheel_ML.transform.Rotate(wheelRotation, 0f, 0f);
            wheel_MR.transform.Rotate(wheelRotation, 0f, 0f);
        }


        carBody.transform.localEulerAngles = new Vector3(-accel, 0f, rotate);

        if(canPlayAniamtions)
        {
            float distace = (Vector3.Distance(transform.position, oldPos)) * 10f * randomeValue;

            GetComponent<Animation>().Play("walking", PlayMode.StopAll);
            GetComponent<Animation>()["walking"].speed = distace * animspeed;
        }

        oldPos = transform.position;
    }
}
