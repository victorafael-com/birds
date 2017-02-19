using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalManager : MonoBehaviour {
	public enum CameraModes
	{
		birdFollow = 0,
		freeFlight = 1,
		stationary = 2
	}

	public static GlobalManager instance;

	public MenuDisplay menu;
	public MenuDisplay vrMenu;

	[Header("CameraModes")]
	public BirdFollowCamera birdFollowCamera;
	public FreeFlightCamera freeFlightCamera;
	public StationaryCamera stationaryCamera;

	void Awake(){
		instance = this;
	}
	// Use this for initialization
	void Start () {
		UpdateMode((CameraModes) PlayerPrefs.GetInt ("SelectedMode", (int)CameraModes.birdFollow));
	}

	public void UpdateMode(CameraModes newMode){
		Debug.Log ("Updating status to " + newMode);
		birdFollowCamera.enabled = newMode == CameraModes.birdFollow;
		freeFlightCamera.enabled = newMode == CameraModes.freeFlight;
		stationaryCamera.enabled = newMode == CameraModes.stationary;

		PlayerPrefs.SetInt ("SelectedMode", (int)newMode);

		vrMenu.UpdateButtons (newMode);
		menu.UpdateButtons (newMode);
	}
	public void ToggleState(){
		menu.Toggle ();
	}
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.Escape)) {
			ToggleState ();
		}
	}
}
