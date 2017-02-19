using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuDisplay : MonoBehaviour {

	public float fadeTime = 0.4f;
	public bool isVR = false;
	private CanvasGroup canvasGroup;
	private bool visible;

	public CameraModeButton[] buttons;

	// Use this for initialization
	void Awake () {
		canvasGroup = GetComponent<CanvasGroup> ();
		canvasGroup.alpha = 0;
		gameObject.SetActive (false);
	}
	public void Toggle(){
		if (!gameObject.activeSelf && isVR) {
			transform.parent.localRotation = Camera.main.transform.localRotation;
		}
		gameObject.SetActive (true);
		StopAllCoroutines ();
		StartCoroutine (FadeTo (visible ? 0 : 1));
	}
	public void UpdateButtons(GlobalManager.CameraModes newMode){
		foreach (var button in buttons) {
			button.Interactable = button.mode != newMode;
		}
	}

	IEnumerator FadeTo(float val){
		float current = canvasGroup.alpha;
		visible = true;

		do {
			yield return new WaitForEndOfFrame ();
			current = Mathf.MoveTowards (current, val, Time.unscaledDeltaTime / fadeTime);
			canvasGroup.alpha = current;
			Time.timeScale = 1 - current;
		} while(current != val);

		canvasGroup.alpha = val;
		Time.timeScale = 1 - val;

		if (val == 0) {
			visible = false;
			gameObject.SetActive (false);
		}
	}
}
