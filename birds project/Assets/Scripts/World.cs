using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class World : MonoBehaviour {
    [System.Serializable]
    public class WorldSpawnDetail {
        public BirdAgentConfig cfg;
        public Vector3 position;
        public float radius;
        public int ammount;
        public int flockId;
    }
    public static World instance;

    public WorldSpawnDetail[] spawns;

    [HideInInspector]
    public Dictionary<BirdType, List<BirdAgent>> birds;

    public float boundariesRadius = 300;
    public GameObject bloodPrefab;

	// Use this for initialization
	void Awake () {
        instance = this;
	}
	void Start() {
        birds = new Dictionary<BirdType, List<BirdAgent>>();
        foreach(var wsd in spawns) { 
            SpawnBirds(wsd);
        }

        foreach (BirdAgent b in birds[BirdType.prey]) {
            b.SetWatchedBirds(birds[BirdType.predator]);
        }
    }
	// Update is called once per frame
	void Update () {
	
	}

    public void SpawnBirds(WorldSpawnDetail spawnDetail) {
        if (!birds.ContainsKey(spawnDetail.cfg.type)) {
            birds.Add(spawnDetail.cfg.type, new List<BirdAgent>());
        }
        for(var i = 0; i < spawnDetail.ammount; i++) {
            
            Vector3 pos = spawnDetail.position + Random.insideUnitSphere * spawnDetail.radius;
            GameObject g = Instantiate<GameObject>(spawnDetail.cfg.prefab);
            g.transform.position = pos;

            var ba = g.GetComponent<BirdAgent>();
            ba.config = spawnDetail.cfg;
            ba.flockID = spawnDetail.flockId;
            birds[spawnDetail.cfg.type].Add(ba);
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(Vector3.up * 15, boundariesRadius);

        Gizmos.color = Color.green;
        foreach(var s in spawns) {
            Gizmos.DrawWireSphere(s.position, s.radius);
        }
    }
    public List<BirdAgent> GetNeighbours(BirdAgent agent, float radius) {
        return birds[agent.config.type].Where(b => b != agent && Vector3.Distance(agent.transform.position, b.transform.position) <= radius && b.flockID == agent.flockID).ToList();
    }
    public void KillBird(BirdAgent b, bool destroyBody) {
        b.alive = false;
        birds[b.config.type].Remove(b);
        if (destroyBody) {
            foreach(Transform t in b.transform) { 
                t.gameObject.SetActive(false);
            }

            Destroy(Instantiate(bloodPrefab, b.transform.position, Quaternion.identity), 10);
        }
        Destroy(b.gameObject, 4);

        string debug = "Killed bird. current count: ";
        foreach (var kvp in birds) {
            debug += "[" + kvp.Key.ToString() + "]: " + kvp.Value.Count + " ";
        }
        Debug.Log(debug);
    }

    //Texture2D whiteDot;
    //void OnGUI() {
    //    if(whiteDot == null) {
    //        whiteDot = new Texture2D(1, 1);
    //        whiteDot.SetPixel(0, 0, Color.white);
    //        whiteDot.Apply();
    //    }
    //    GUI.color = Color.black;
    //    Rect cr = new Rect(Camera.main.transform.position.x-3, Camera.main.transform.position.z-3, 6, 6);
    //    GUI.DrawTexture(cr, whiteDot);
    //    GUI.color = Color.white;
    //    cr.x += 1;
    //    cr.y += 1;
    //    cr.width -= 2;
    //    cr.height -= 2;
    //    GUI.DrawTexture(cr, whiteDot);
    //    //GUI.DrawTexture()
    //    foreach(var kvp in birds) { 
    //        foreach(var b in kvp.Value) {
    //            Rect r = new Rect(
    //                b.transform.position.x-1,
    //                b.transform.position.z-1,
    //                2, 2);
    //            r.x += Screen.width * 0.5f;
    //            r.y += Screen.height * 0.5f;
    //            GUI.color = Color.Lerp(Color.green, Color.red, Mathf.InverseLerp(0, 80, b.transform.position.y));
    //            GUI.DrawTexture(r, whiteDot);
    //        }
    //    }
    //}
}
