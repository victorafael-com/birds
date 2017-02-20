using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuDisplay : MonoBehaviour {

	[Header("Structure")]
	public float fadeTime = 0.4f;
	public CameraModeButton[] buttons;

	[Header("VR Support")]
	public bool isVR = false;
	public CanvasGroup reticleCanvasGroup;

	private CanvasGroup canvasGroup;
	private bool visible;


	[Header("Bird Count")]
	public Text birdCountDisplay;
	public Text removeBirdLabel;
	public int removeBirdCount = 5;

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
	void OnEnable(){
		UpdateBirdCount ();
	}

	void UpdateBirdCount(){
		int count = World.instance.birds [BirdType.prey].Count + World.instance.birds [BirdType.predator].Count;
		birdCountDisplay.text = count.ToString ();

		removeBirdLabel.text = "- " + Mathf.Min(count, removeBirdCount);
	}

	public void UpdateBirdAmmount(int count){
		GlobalManager.instance.UpdateBirdAmmount (count);
		UpdateBirdCount ();
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

	void Update(){
		var system = UnityEngine.EventSystems.EventSystem.current;
		var builder = new System.Text.StringBuilder ();
		builder.AppendLine("Already Selecting: " + system.alreadySelecting);
		builder.AppendLine ("Current Input Module: " + system.currentInputModule.GetType ().ToString ());
		builder.AppendLine ("Current Sel. GameObj: " + (system.currentSelectedGameObject == null ? "Null" : system.currentSelectedGameObject.name));
	}
}
