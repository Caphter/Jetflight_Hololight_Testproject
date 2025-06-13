using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[ExecuteInEditMode]
public class movingParts : MonoBehaviour
{
    [Header("Game Objects")]
    public GameObject Breake;
    public GameObject Flap_L;
    public GameObject Flap_R;
    public GameObject Hoehe_L;
    public GameObject Hoehe_R;
    public GameObject Quer_L;
    public GameObject Quer_R;
    public GameObject Seite_L;
    public GameObject Seite_R;
    public GameObject Haube;

    [Header("Public Values to Moving, max Value = 1.0")]
    [Range(0, 1)] public float breakeValue;
    [Range(0, 1)] public float flapsValue;
    [Range(-1, 1)] public float pitchVAlue;
    [Range(-1, 1)] public float rollValue;
    [Range(-1, 1)] public float yawValue;
    [Range(0, 1)] public float haubeVAlue;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MovingParts();
    }

    private void MovingParts()
    {
        Breake.transform.localEulerAngles = new Vector3(breakeValue * 45.0f, 0f, 0f);

        Flap_L.transform.localEulerAngles = new Vector3(flapsValue * 40f * -1f, 0f, 0f);
        Flap_R.transform.localEulerAngles = new Vector3(flapsValue * 40f * -1f, 0f, 0f);

        Hoehe_L.transform.localEulerAngles = new Vector3(pitchVAlue * 25f, 0f, 0f);
        Hoehe_R.transform.localEulerAngles = new Vector3(pitchVAlue * 25f, 0f, 0f);

        Quer_L.transform.localEulerAngles = new Vector3(rollValue * 20f * -1f, -20f, 0f);
        Quer_R.transform.localEulerAngles = new Vector3(rollValue * 20f      , 20f, 0f);

        Seite_L.transform.localEulerAngles = new Vector3(-15f + (0.942f * yawValue), 20f * yawValue * -1f, 5.07f * yawValue);
        Seite_R.transform.localEulerAngles = new Vector3(-15f + (0.942f * yawValue), 20f * yawValue * -1f, 5.07f * yawValue);

        Haube.transform.localEulerAngles = new Vector3(45f * haubeVAlue * -1f, 0f, 0f);
    }
}
