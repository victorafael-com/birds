using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class LowPolyTerrain : MonoBehaviour {
	public float unitsPerPixel = 1;
	public float minHeight = 0;
	public float maxHeight = 100;

	public List<Color> colors;
	public List<float> heightLimit;
	
	public Texture2D referenceTexture = null;

	/// <summary>
	/// Gets a terrain position relative to two percentages
	/// </summary>
	/// <returns>The terrain position.</returns>
	/// <param name="x">percent in X.</param>
	/// <param name="z">percent in Z.</param>
	public Vector3 GetTerrainPosition(float x, float z){
		Mesh m = GetComponent<MeshFilter> ().sharedMesh;
		Vector3 pos = new Vector3 (
			Mathf.Lerp (m.bounds.min.x, m.bounds.max.x, x),
			maxHeight,
			Mathf.Lerp (m.bounds.min.z, m.bounds.max.z, z));

		MeshCollider collider = GetComponent<MeshCollider> ();

		RaycastHit hitInfo;
		Ray ray = new Ray (transform.TransformPoint(pos), Vector3.down);


		if (collider.Raycast(ray, out hitInfo, maxHeight)) {
			return transform.InverseTransformPoint (hitInfo.point);
		} else {
			return pos;
		}
	}
}
