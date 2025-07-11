using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamNavigation : MonoBehaviour 
{
	[Header("Setup the Camera Rotation, < Smooth = < Smoothing")]
	public float RotationSpeed = 1.0f;
	public float RotateSmoth = 10.0f;
	private float RotVertical;
	private float RotVerticalSlerp;
	private float RotHorizontal;
	private float RotHorizontalSlerp;
	private float RotVelocityVertical = 0.0f;
	private float RotVelocityHorizontal = 0.0f;

	[Header("Setup the Camera Transform, < Smooth = < Smoothing")]
	public float MoveSpeed = 1.0f;
	public float MoveSmoth = 10.0f;
	private float MoveForward;
	private float MoveForwardSlerp;
	private float MoveVelocityForward = 0.0f;
	private float MoveSide;
	private float MoveSideSlerp;
	private float MOveVelocitySide = 0.0f;

	public bool canFly = false;
	public float walkHight = 1.6f;

	// Use this for initialization
	void Start () 
	{
		RotVertical = transform.localEulerAngles.z;
		RotHorizontal = transform.localEulerAngles.y;
	}
	
	// Update is called once per frame
	void Update () 
	{
		CamRotation ();
		CamMoving ();
	}

	void CamRotation()
	{
		if (Input.GetMouseButton(1))
		{
			RotVertical -= RotationSpeed * Input.GetAxis("Mouse Y");
			RotHorizontal += RotationSpeed * Input.GetAxis("Mouse X");
		}

		if (RotVertical >= 90.0) RotVertical = 89.9f;
		if (RotVertical <= -90.0) RotVertical = -89.9f;

		RotVerticalSlerp = Mathf.SmoothDamp (RotVerticalSlerp, RotVertical, ref RotVelocityVertical, RotateSmoth * Time.deltaTime);
		RotHorizontalSlerp = Mathf.SmoothDamp (RotHorizontalSlerp, RotHorizontal, ref RotVelocityHorizontal, RotateSmoth * Time.deltaTime);

		transform.eulerAngles = new Vector3 (RotVerticalSlerp, RotHorizontalSlerp, 0.0f);
	}

	void CamMoving()
	{
		
		if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow)) 		MoveForward = MoveSpeed * Time.deltaTime;
		if (Input.GetKeyUp (KeyCode.W) || Input.GetKeyUp (KeyCode.UpArrow)) 	MoveForward = 0f;
		if (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow)) 		MoveForward = MoveSpeed * -Time.deltaTime;
		if (Input.GetKeyUp (KeyCode.S) || Input.GetKeyUp (KeyCode.DownArrow))	MoveForward = 0f;

		if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow))		MoveSide = MoveSpeed * -Time.deltaTime;
		if (Input.GetKeyUp (KeyCode.A) || Input.GetKeyUp (KeyCode.LeftArrow))	MoveSide = 0f;
		if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow))		MoveSide = MoveSpeed * Time.deltaTime;
		if (Input.GetKeyUp (KeyCode.D) || Input.GetKeyUp (KeyCode.RightArrow))	MoveSide = 0f;



		MoveForwardSlerp = Mathf.SmoothDamp (MoveForwardSlerp, MoveForward, ref MoveVelocityForward, MoveSmoth * Time.deltaTime);
		MoveSideSlerp = Mathf.SmoothDamp (MoveSideSlerp, MoveSide, ref MOveVelocitySide, MoveSmoth * Time.deltaTime);


		transform.Translate (MoveSideSlerp, 0f, MoveForwardSlerp);
		if(!canFly) transform.position = new Vector3 (transform.position.x, walkHight, transform.position.z);
	}
}
