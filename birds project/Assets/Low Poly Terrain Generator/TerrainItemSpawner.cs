using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LowPolyTerrain))]
public class TerrainItemSpawner : MonoBehaviour {
	public GameObject[] prefabs;
	[Tooltip("Set only if all objects shares the same material")]
	public Material[] materials;

	public float minHeight = 0;
	public float maxHeight = 600;

	public int itemAmmount = 500;

	public void Spawn(){
		Transform root = transform.FindChild ("Spawned Items");
		if (root != null) {
			foreach (Transform t in root) {
				DestroyImmediate (t.gameObject);
			}
		} else {
			root = new GameObject ("Spawned Items").transform;
			root.transform.parent = transform;
		}

		LowPolyTerrain terrain = GetComponent<LowPolyTerrain> ();

		if (maxHeight <= minHeight || minHeight > maxHeight) {
			minHeight = terrain.minHeight;
			maxHeight = terrain.maxHeight;
		}
		if (minHeight > terrain.maxHeight) {
			minHeight = terrain.minHeight;
		}
		if (maxHeight < terrain.minHeight) {
			maxHeight = terrain.maxHeight;
		}

		root.transform.localPosition = Vector3.zero;
		root.transform.localScale = Vector3.one;
		for (int i = 0; i < itemAmmount; i++) { 
			GameObject prefab = prefabs [Random.Range (0, prefabs.Length)];
			GameObject newObj = Instantiate<GameObject> (prefab);
			newObj.transform.parent = root;

			int count = 0;
			Vector3 pos;
			do {
				pos = terrain.GetTerrainPosition(Random.value, Random.value);
			} while((pos.y < minHeight || pos.y > maxHeight ) && ++count < 5);

			newObj.transform.localPosition = pos;

			if (materials.Length > 0) {
				Material material = materials[Random.Range(0, materials.Length)];
				newObj.GetComponent<Renderer> ().material = material;
			}
		}
	}
}
