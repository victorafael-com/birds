using UnityEngine;
using System.Collections;
using UnityEngine.VR;

public class BirdFollowCamera : MonoBehaviour {
    public float birdSwapInterval = 5;
    public float moveSpeed = 4;

    private Transform target;
    private Camera cam;
    private Vector3 originalPos;
    public Vector3 zoomPos;
    public BirdType searchType;

	private bool rotate = true;

	// Use this for initialization
	void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cam = GetComponentInChildren<Camera>();
        originalPos = cam.transform.localPosition;

		if (VRSettings.enabled) {
			rotate = false;
		}
	}
	
	// Update is called once per frame
	void LateUpdate () {
        
//        if (Input.GetMouseButtonUp(1)) {
//            target = World.instance.birds[searchType].GetRandom().transform;
//        }
		if (Input.GetKeyUp (KeyCode.Escape)) {
			target = World.instance.birds[searchType].GetRandom().transform;
		}

		if (target != null) {
			transform.position = Vector3.MoveTowards (transform.position, target.transform.position, moveSpeed * Time.deltaTime * (Input.GetMouseButton (0) ? 5 : 1));
			if (rotate) {
				transform.rotation = Quaternion.RotateTowards (transform.rotation, target.rotation, 45 * Time.deltaTime);
			}
		} else {
			target = World.instance.birds[searchType].GetRandom().transform;
			transform.position = target.transform.position;
		}
//        if (!Input.GetMouseButton(0) || target == null) {
//            transform.Rotate(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
//        } else {
//        }

//        if (Input.GetMouseButton(2)) {
//            cam.transform.localPosition = Vector3.MoveTowards(cam.transform.localPosition, zoomPos, 5 * Time.deltaTime);
//        } else {
//            cam.transform.localPosition = Vector3.MoveTowards(cam.transform.localPosition, originalPos, 5 * Time.deltaTime);
//        }
	}
}
