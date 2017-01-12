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

	public float distanceTableUpdateInterval = 0.4f;
	private float distanceTableMarker = 0;
	private Dictionary<int, float> distanceTable;

	// Use this for initialization
	void Awake () {
        instance = this;
	}

	void Start() {
		distanceTable = new Dictionary<int, float> ();
        birds = new Dictionary<BirdType, List<BirdAgent>>();
//
//		WorldSpawnDetail w = new WorldSpawnDetail () {
//			ammount = 55,
//			cfg = spawns [0].cfg,
//			flockId = 1,
//			position = Vector3.up * 40,
//			radius = 1
//		};
//		boundariesRadius = 15;
//		SpawnBirds (w);
        foreach(var wsd in spawns) { 
            SpawnBirds(wsd);
        }

        foreach (BirdAgent b in birds[BirdType.prey]) {
            b.SetWatchedBirds(birds[BirdType.predator]);
        }
    }
	void Update(){
		distanceTableMarker += Time.deltaTime;
		if(distanceTableMarker > distanceTableUpdateInterval)
			distanceTable.Clear ();
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

	public float BirdDistance(BirdAgent a, BirdAgent b){
		int key;
		if (a.Id < b.Id) {
			key = a.Id * 10000 + b.Id;
		} else {
			key = b.Id * 10000 + a.Id;
		}
		if(distanceTable.ContainsKey(key)){
			return (float)distanceTable [key];
		}else{
			float dist = Vector3.Distance (a.position, b.position);
			distanceTable.Add (key, dist);
			return dist;
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
		var ret = new List<BirdAgent> ();
		var list = birds [agent.config.type].ToArray();
		int index = System.Array.IndexOf (list, agent);
		for (var i = 0; i < list.Length; i++) {
			if (i == index)
				continue;
			if (BirdDistance (agent, list [i]) <= radius && agent.flockID == list [i].flockID)
				ret.Add (list [i]);
		}
		return ret;
        //return birds[agent.config.type].Where(b => b != agent && Vector3.Distance(agent.transform.position, b.transform.position) <= radius && b.flockID == agent.flockID).ToList();
    }
	public List<BirdAgent> GetNeighbours(BirdAgent agent, List<BirdAgent> searchList, float radius){
		var ret = new List<BirdAgent> ();
		for (var i = 0; i < searchList.Count; i++) {
			if (BirdDistance (agent, searchList [i]) <= radius && agent.flockID == searchList [i].flockID)
				ret.Add (searchList [i]);
		}
		return ret;
	}
    public void KillBird(BirdAgent b, bool destroyBody) {
        b.alive = false;
        birds[b.config.type].Remove(b);
        if (destroyBody) {
			b.DestroyBody ();
            Destroy(Instantiate(bloodPrefab, b.transform.position, Quaternion.identity), 10);
        }
        Destroy(b.gameObject, 4);

        string debug = "Killed bird. current count: ";
        foreach (var kvp in birds) {
            debug += "[" + kvp.Key.ToString() + "]: " + kvp.Value.Count + " ";
        }
        Debug.Log(debug);
    }
}
