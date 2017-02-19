using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainItemSpawner))]
public class TerrainItemSpawnerEditor : Editor {
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		if (GUILayout.Button ("Spawn")) {
			((TerrainItemSpawner)target).Spawn ();
		}
	}
}
