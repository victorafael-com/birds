using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LowPolyTerrain : MonoBehaviour {
	public float unitsPerPixel = 1;
	public float minHeight = 0;
	public float maxHeight = 100;

	public List<Color> colors;
	public List<float> heightLimit;
	
	public Texture2D referenceTexture = null;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
