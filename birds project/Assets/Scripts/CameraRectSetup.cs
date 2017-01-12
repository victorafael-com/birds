using UnityEngine;
using System.Collections;

public class CameraRectSetup : MonoBehaviour {
	public enum pos{
		topLeft,
		topRight,
		bottomLeft,
		bottomRight
	}
	public pos position;
	public Vector2 offset;
	public Vector2 rectSize;
	// Use this for initialization
	void Start () {
		Camera c = GetComponentInChildren<Camera> ();
		Vector2 off = new Vector2( offset.x / Screen.width, offset.y / Screen.height);
		Vector2 size = new Vector2 ();
		size.x = rectSize.x / Screen.width;
		size.y = rectSize.y / Screen.height;

		switch (position) {
		case pos.bottomLeft:
			c.rect = new Rect (
				off.x,
				off.y,
				size.x,
				size.y
			);
			break;
		case pos.bottomRight:
			c.rect = new Rect (
				1 - size.x - off.x,
				off.y,
				size.x,
				size.y
			);
			break;
		case pos.topLeft:
			c.rect = new Rect (
				off.x,
				1 - size.y - off.y,
				size.x,
				size.y
			);
			break;
		case pos.topRight:
			c.rect = new Rect (
				1 - size.x - off.x,
				1 - size.y - off.y,
				size.x,
				size.y
			);
			break;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
