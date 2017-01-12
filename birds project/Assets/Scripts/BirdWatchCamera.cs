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

	private float startDistance = 0;
	private float calcFieldOfView = 0;
	private float lastChange = -30;

	// Use this for initialization
	void Start () {
		rotateTransform = new GameObject ("CamRotHelper").transform;
		rotateTransform.transform.position = transform.position;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		cam = GetComponentInChildren<Camera>();
		originalPos = cam.transform.localPosition;

		Input.simulateMouseWithTouches = false;
	}

	void Update(){
		if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Space)) {
			if (Time.time - lastChange < 0.8f) {
				searchType = (searchType == BirdType.predator ? BirdType.prey : BirdType.predator);
				lastChange = -30;
			}
			lastChange = Time.time;
			target = World.instance.birds[searchType].GetRandom().transform;
		}

		if (Input.touchCount == 2) {
			float distance = Vector2.Distance (Input.GetTouch (0).position, Input.GetTouch (1).position);
			if (startDistance == 0) {
				startDistance = distance;
			}
			calcFieldOfView = cam.fieldOfView;
			calcFieldOfView += ((distance / startDistance) - 1) * moveSpeed * 0.4f * Time.deltaTime;
			cam.fieldOfView = Mathf.Clamp (calcFieldOfView, minFOV, maxFOV);
		} else {
			startDistance = 0;
		}

		if (Input.mousePresent) {
			if (Input.GetMouseButton (1)) {
				cam.fieldOfView -= Time.deltaTime * moveSpeed;
				if (cam.fieldOfView < minFOV)
					cam.fieldOfView = minFOV;
			} else if (Input.GetMouseButton (0)) {
				cam.fieldOfView += Time.deltaTime * moveSpeed;
				if (cam.fieldOfView > maxFOV)
					cam.fieldOfView = maxFOV;
			}
		}
	}

	// Update is called once per frame
	void LateUpdate () {
		if (target != null) {
			rotateTransform.LookAt (target);
			transform.rotation = Quaternion.RotateTowards (transform.rotation, rotateTransform.rotation, 180 * Time.deltaTime);
		}
	}
}
