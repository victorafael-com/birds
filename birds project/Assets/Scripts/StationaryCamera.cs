using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationaryCamera : MonoBehaviour {
	public Vector3 floorOffset;
	private Camera usedCamera;
	// Use this for initialization
	void Start () {
		usedCamera = GetComponentInChildren<Camera> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonUp (0)) {
			Ray r = Camera.main.ViewportPointToRay (new Vector3 (0.5f, 0.5f));
			RaycastHit hit;

			if (Physics.Raycast (r, out hit)) {
				transform.position = hit.point + floorOffset;// + usedCamera.transform.TransformPoint(usedCamera.transform.localPosition);
				transform.Translate(
					-(usedCamera.transform.right * usedCamera.transform.localPosition.x +
						usedCamera.transform.up * usedCamera.transform.localPosition.y +
						usedCamera.transform.forward * usedCamera.transform.localPosition.z)
				);
			}
		}
	}
}
