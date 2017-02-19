using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LowPolyTerrain))]
public class LowPolyTerrainEditor : Editor {

	private const int ColorMaterialWidth = 4;

	public override void OnInspectorGUI ()
	{
		var lowPolyTerrain = target as LowPolyTerrain;

		lowPolyTerrain.unitsPerPixel = EditorGUILayout.FloatField ("Pixel size in units", lowPolyTerrain.unitsPerPixel);
		lowPolyTerrain.referenceTexture = EditorGUILayout.ObjectField ("Terrain R/W Texture", lowPolyTerrain.referenceTexture, typeof(Texture2D),false) as Texture2D;

		if(lowPolyTerrain.referenceTexture != null){
			TextureImporter importer = AssetImporter.GetAtPath (AssetDatabase.GetAssetPath (lowPolyTerrain.referenceTexture)) as TextureImporter;
			if (!importer.isReadable) {
				importer.isReadable = true;
				importer.filterMode = FilterMode.Point;
				importer.SaveAndReimport ();
			}
		}

		lowPolyTerrain.minHeight = EditorGUILayout.FloatField ("Min height (black)", lowPolyTerrain.minHeight);
		lowPolyTerrain.maxHeight = EditorGUILayout.FloatField ("Max height (white)", lowPolyTerrain.maxHeight);

		EditorGUILayout.Space ();

		if (lowPolyTerrain.colors == null || lowPolyTerrain.colors.Count == 0) {
			lowPolyTerrain.colors = new List<Color> ();
			lowPolyTerrain.heightLimit = new List<float> ();
			lowPolyTerrain.colors.Add (Color.white);
			lowPolyTerrain.heightLimit.Add (1);
		}

		EditorGUILayout.LabelField ("Color ranges");
		float lastMax = 0;
		for (int i = 0; i < lowPolyTerrain.colors.Count; i++) {
			GUI.enabled = true;
			GUILayout.BeginHorizontal ();
			lowPolyTerrain.colors [i] = EditorGUILayout.ColorField (lowPolyTerrain.colors [i]);
			GUI.enabled = i < lowPolyTerrain.colors.Count - 1;

			lowPolyTerrain.heightLimit [i] = EditorGUILayout.Slider (lowPolyTerrain.heightLimit [i], 0, 1);
			if (lowPolyTerrain.heightLimit [i] < lastMax) {
				lowPolyTerrain.heightLimit [i] = lastMax;
			}
			lastMax = lowPolyTerrain.heightLimit [i];
			if (GUILayout.Button ("x")) {
				lowPolyTerrain.colors.RemoveAt (i);
				lowPolyTerrain.heightLimit.RemoveAt (i);
				GUILayout.EndHorizontal ();
				break;
			}
			GUILayout.EndHorizontal ();
		}
		GUI.enabled = lowPolyTerrain.colors.Count < (ColorMaterialWidth * ColorMaterialWidth);
		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button ("add color")) {
			lowPolyTerrain.colors.Add (lowPolyTerrain.colors [lowPolyTerrain.colors.Count - 1]);
			lowPolyTerrain.heightLimit.Add (1);
		}
		GUILayout.FlexibleSpace ();
		EditorGUILayout.EndHorizontal ();
		GUI.enabled = true;
		EditorGUILayout.Space ();

		GUI.enabled = lowPolyTerrain.referenceTexture != null;
		if (GUILayout.Button ("Generate")) {
			GenerateTerrain (lowPolyTerrain);
		}
	}

	void GenerateTerrain(LowPolyTerrain lowPolyTerrain){

		int width = lowPolyTerrain.referenceTexture.width;
		int height = lowPolyTerrain.referenceTexture.height;


		Vector2 initialOffset = -new Vector2 (width * lowPolyTerrain.unitsPerPixel, height * lowPolyTerrain.unitsPerPixel) / 2;

		Color[] pixels = lowPolyTerrain.referenceTexture.GetPixels ();
		Vector3[] referenceVertices = new Vector3[pixels.Length];

		List<Vector3> realVertices = new List<Vector3> ();
		List<Vector2> uvs = new List<Vector2> ();
		List<int> triangles = new List<int> ();

		//Setups all reference vertices on its places
		for (int i = 0; i < pixels.Length; i++) {
			int x = i % width;
			int z = i / width;

			referenceVertices [i] = new Vector3 (
				initialOffset.x + x * lowPolyTerrain.unitsPerPixel,
				Mathf.Lerp(lowPolyTerrain.minHeight, lowPolyTerrain.maxHeight, pixels[i].r),
				initialOffset.y + z * lowPolyTerrain.unitsPerPixel);
		}

		int currentVertice = 0;
		for (int x = 0; x < width - 1; x++) {
			for (int y = 0; y < height - 1; y++) {
				int current = GetVerticeIndex (x, y, width);
				int right = GetVerticeIndex (x + 1, y, width);
				int bottom = GetVerticeIndex (x, y + 1, width);
				int diagonal = GetVerticeIndex (x + 1, y + 1, width);

				Vector2 quadUV = GetColorUV (lowPolyTerrain, referenceVertices [current], referenceVertices [right], referenceVertices [bottom], referenceVertices [diagonal]);

				//Populates the real vertices list with the pre calculated vertices
				realVertices.Add (referenceVertices [current]); 
				realVertices.Add (referenceVertices [right]);
				realVertices.Add (referenceVertices [bottom]);
				realVertices.Add (referenceVertices [diagonal]);

				//Set uv for the 4 new vertices
				uvs.Add (quadUV);
				uvs.Add (quadUV);
				uvs.Add (quadUV);
				uvs.Add (quadUV);

				//Replaces the original ids with the new vertices ids
				current = currentVertice++; 
				right = currentVertice++;
				bottom = currentVertice++;
				diagonal = currentVertice++;

				triangles.Add (current);
				triangles.Add (bottom);
				triangles.Add (right);
				triangles.Add (right);
				triangles.Add (bottom);
				triangles.Add (diagonal);
			}
		}

		MeshFilter filter = lowPolyTerrain.GetComponent<MeshFilter> ();
		if (lowPolyTerrain.GetComponent<MeshCollider> () != null) {
			var collider = lowPolyTerrain.GetComponent<MeshCollider> ();
			DestroyImmediate (collider);
			lowPolyTerrain.gameObject.AddComponent<MeshCollider> ();
		}

		Mesh mesh = new Mesh ();
		mesh.SetVertices(realVertices);
		mesh.SetTriangles(triangles,0);
		mesh.SetUVs (0, uvs);
		mesh.RecalculateNormals ();

		filter.sharedMesh = mesh;

		//Create Texture
		Texture2D t = new Texture2D(ColorMaterialWidth, ColorMaterialWidth);
		Color[] renderPixels = new Color[ColorMaterialWidth * ColorMaterialWidth];
		for (int i = 0; i < lowPolyTerrain.colors.Count; i++) {
			renderPixels [i] = lowPolyTerrain.colors [i];
		}
		t.SetPixels (renderPixels);
		t.Apply ();

		lowPolyTerrain.GetComponent<MeshRenderer> ().sharedMaterial.mainTexture = t;
	}

	int GetVerticeIndex(int x, int y, int width){
		return y * width + x;
	}

	Vector2 GetColorUV(LowPolyTerrain terrain, params Vector3[] vertices){
		int id = GetColorId (terrain, vertices);
		int x = id % ColorMaterialWidth;
		int y = id / ColorMaterialWidth;

		float unitSize = 1 / (float)ColorMaterialWidth;

		Vector2 result = new Vector2 (x * unitSize + unitSize * 0.5f, y * unitSize + unitSize * 0.5f);
		return result;
	}

	int GetColorId(LowPolyTerrain terrain, params Vector3[] vertices){
		float y = 0;

		for (var i = 0; i < vertices.Length; i++)
			y += vertices [i].y;

		y /= vertices.Length;
		float heightLerp = Mathf.InverseLerp (terrain.minHeight, terrain.maxHeight, y);
		for (int i = 0; i < terrain.heightLimit.Count; i++) {
			if (heightLerp <= terrain.heightLimit [i]) {
				return i;
			}
		}
		return terrain.heightLimit.Count - 1;
	}
}
