using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeFlightCamera : MonoBehaviour {
	public float maxSpeed = 12;
	private Camera usedCamera;
	// Use this for initialization
	void Start () {
		usedCamera = GetComponentInChildren<Camera> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton (0)) {
			transform.Translate (usedCamera.transform.forward * Time.deltaTime * maxSpeed * Mathf.InverseLerp (0, Screen.height, Input.mousePosition.y));
		}
	}
}
