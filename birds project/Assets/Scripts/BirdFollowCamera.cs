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
        if (Input.GetMouseButtonUp(0)) {
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
	}
}
