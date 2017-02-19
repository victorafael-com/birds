using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CameraModeButton : MonoBehaviour {
	public bool Interactable{
		get{
			return button.interactable;
		}
		set{
			button.interactable = value;
		}
	}

	public GlobalManager.CameraModes mode;
	private Button button;

	void Awake(){
		button = GetComponent<Button> ();
	}

	public void Click(){
		GlobalManager.instance.UpdateMode (mode);
		GlobalManager.instance.ToggleState ();
	}
}
