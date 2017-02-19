using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LowPolyTerrainWindow : EditorWindow {
	[MenuItem("Low Poly Terrain/Create...")]
	public static void OpenWindow(){
		LowPolyTerrainWindow window = new LowPolyTerrainWindow ();
		window.Show ();
		window.titleContent = new GUIContent( "Low Poly Terrain Creator" );
	}

	private float unitsPerPixel = 1;
	private float minHeight = 0;
	private float maxHeight = 100;
	private Texture2D referenceTexture = null;
	void OnGUI(){
		unitsPerPixel = EditorGUILayout.FloatField ("Pixel size in units", unitsPerPixel);
		referenceTexture = EditorGUILayout.ObjectField ("Terrain R/W Texture", referenceTexture, typeof(Texture2D)) as Texture2D;

		if(referenceTexture != null){
			TextureImporter importer = AssetImporter.GetAtPath (AssetDatabase.GetAssetPath (referenceTexture)) as TextureImporter;
			if (!importer.isReadable) {
				importer.isReadable = true;
				importer.filterMode = FilterMode.Point;
				importer.SaveAndReimport ();
			}
		}

		minHeight = EditorGUILayout.FloatField ("Min height (black)", minHeight);
		maxHeight = EditorGUILayout.FloatField ("Max height (white)", maxHeight);

		EditorGUILayout.Space ();

		GUI.enabled = referenceTexture != null;
		if (GUILayout.Button ("Generate")) {
			GenerateTerrain ();
		}
	}

	void GenerateTerrain(){

		int width = referenceTexture.width;
		int height = referenceTexture.height;


		Vector2 initialOffset = -new Vector2 (width * unitsPerPixel, height * unitsPerPixel) / 2;

		Color[] pixels = referenceTexture.GetPixels ();
		Vector3[] referenceVertices = new Vector3[pixels.Length];

		List<Vector3> realVertices = new List<Vector3> ();
		List<int> triangles = new List<int> ();

		//Setups all reference vertices on its places
		for (int i = 0; i < pixels.Length; i++) {
			int x = i % width;
			int z = i / width;

			referenceVertices [i] = new Vector3 (
				initialOffset.x + x * unitsPerPixel,
				Mathf.Lerp(minHeight, maxHeight, pixels[i].r),
				initialOffset.y + z * unitsPerPixel);
		}

		int currentVertice = 0;
		for (int x = 0; x < width - 1; x++) {
			for (int y = 0; y < height - 1; y++) {
				int current = GetVerticeIndex (x, y, width);
				int right = GetVerticeIndex (x + 1, y, width);
				int bottom = GetVerticeIndex (x, y + 1, width);
				int diagonal = GetVerticeIndex (x + 1, y + 1, width);

				//Populates the real vertices list with the pre calculated vertices
				realVertices.Add (referenceVertices [current]); 
				realVertices.Add (referenceVertices [right]);
				realVertices.Add (referenceVertices [bottom]);
				realVertices.Add (referenceVertices [diagonal]);

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

/*				triangles [currentPos++] = current;
				triangles [currentPos++] = bottom;
				triangles [currentPos++] = right;
				triangles [currentPos++] = right;
				triangles [currentPos++] = bottom;
				triangles [currentPos++] = diagonal;*/
			}
		}

		GameObject generated;
		MeshFilter filter;
		if (Selection.activeGameObject != null) {
			generated = Selection.activeGameObject;
			filter = generated.GetComponent<MeshFilter> ();
		} else {
			generated = new GameObject ("Generated Terrain");
			filter = generated.AddComponent<MeshFilter> ();
			generated.AddComponent<MeshRenderer> ();
		}

		Mesh mesh = new Mesh ();
		mesh.SetVertices(realVertices);
		mesh.SetTriangles(triangles,0);
		mesh.RecalculateNormals ();

		filter.sharedMesh = mesh;
	}

	void OldGenerateTerrain(){
		int width = referenceTexture.width;
		int height = referenceTexture.height;

		Vector2 initialOffset = -new Vector2 (width * unitsPerPixel, height * unitsPerPixel) / 2;

		Color[] pixels = referenceTexture.GetPixels ();
		Vector3[] vertices = new Vector3[pixels.Length];

		for (int i = 0; i < pixels.Length; i++) {
			int x = i % width;
			int z = i / width;

			vertices [i] = new Vector3 (
				initialOffset.x + x * unitsPerPixel,
				Mathf.Lerp(minHeight, maxHeight, pixels[i].r),
				initialOffset.y + z * unitsPerPixel);
		}

		int triangleCount = (width - 1) * (height - 1) * 6;
		int[] triangles = new int[triangleCount];

		int currentPos = 0;
		for (int x = 0; x < width - 1; x++) {
			for (int y = 0; y < height - 1; y++) {
				int current = GetVerticeIndex (x, y, width);
				int right = GetVerticeIndex (x + 1, y, width);
				int bottom = GetVerticeIndex (x, y + 1, width);
				int diagonal = GetVerticeIndex (x + 1, y + 1, width);

				triangles [currentPos++] = current;
				triangles [currentPos++] = bottom;
				triangles [currentPos++] = right;
				triangles [currentPos++] = right;
				triangles [currentPos++] = bottom;
				triangles [currentPos++] = diagonal;
			}
		}

		GameObject generated;
		MeshFilter filter;
		if (Selection.activeGameObject != null) {
			generated = Selection.activeGameObject;
			filter = generated.GetComponent<MeshFilter> ();
		} else {
			generated = new GameObject ("Generated Terrain");
			filter = generated.AddComponent<MeshFilter> ();
			generated.AddComponent<MeshRenderer> ();
		}

		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals ();

		filter.sharedMesh = mesh;

	}

	int GetVerticeIndex(int x, int y, int width){
		return y * width + x;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
