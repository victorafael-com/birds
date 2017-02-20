using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CameraModeButton : MonoBehaviour {
	public bool Interactable{
		get{
			if (button == null)
				button = GetComponent<Button> ();
			return button.interactable;
		}
		set{
			if (button == null)
				button = GetComponent<Button> ();
			button.interactable = value;
		}
	}

	public GlobalManager.CameraModes mode;
	private Button button;

	public void Click(){
		GlobalManager.instance.UpdateMode (mode);
		GlobalManager.instance.ToggleState ();
	}
}
