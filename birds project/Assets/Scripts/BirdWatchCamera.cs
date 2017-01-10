using UnityEngine;
using System.Collections;

public class BirdWatchCamera : MonoBehaviour {
	public float birdSwapInterval = 5;
	public float moveSpeed = 4;

	private Transform target;
	private Transform rotateTransform;
	private Camera cam;
	private Vector3 originalPos;
	public Vector3 zoomPos;
	public BirdType searchType;
	public float minFOV;
	public float maxFOV;

	// Use this for initialization
	void Start () {
		rotateTransform = new GameObject ("CamRotHelper").transform;
		rotateTransform.transform.position = transform.position;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		cam = GetComponentInChildren<Camera>();
		originalPos = cam.transform.localPosition;
	}

	// Update is called once per frame
	void LateUpdate () {

		if (Input.GetKeyUp(KeyCode.Space)) {
			target = World.instance.birds[searchType].GetRandom().transform;
		}

		if (Input.GetMouseButton (1)) {
			cam.fieldOfView -= Time.deltaTime * 8;
			if (cam.fieldOfView < minFOV)
				cam.fieldOfView = minFOV;
		}else if (Input.GetMouseButton (0)) {
			cam.fieldOfView += Time.deltaTime * 8;
			if (cam.fieldOfView > maxFOV)
				cam.fieldOfView = maxFOV;
		}

		if (target != null) {
			rotateTransform.LookAt (target);
			transform.rotation = Quaternion.RotateTowards (transform.rotation, rotateTransform.rotation, 50 * Time.deltaTime);
		}
	}
}
