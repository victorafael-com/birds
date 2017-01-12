using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BirdAgent : MonoBehaviour
{
	private static int CurrentBirdID = 0;

	private const int SEPARATION_SKIPS = 8;
	private const int COHESION_SKIPS = 4;
	private const int ALIGNMENT_SKIPS = 4;
	Vector3 acceleration;
	Vector3 velocity;

	[HideInInspector]
	public BirdAgentConfig config;
	[HideInInspector]
	public int flockID;
	private List<BirdAgent> cohesionNeighbours;
	private List<BirdAgent> separationNeighbours;
	private List<BirdAgent> alignmentNeighbours;
	private List<BirdAgent> watchedBirds;

	private Animator animator;

	public bool alive = true;

	private float velocityVariation;

	private int _id = 0;

	public int Id {
		get {
			if (_id == 0) {
				_id = ++CurrentBirdID;
			}
			return _id;
		}
	}

	[HideInInspector]
	public Vector3 position = Vector3.zero;
	private Vector3 animVector = Vector3.zero;
	private int cohesionSkipCount;
	private int separationSkipCount;
	private int alignmentSkipCount;

	private Vector3 cohesionCache;
	private Vector3 separationCache;
	private Vector3 alignmentCache;
	#if UNITY_EDITOR
	public bool debug;
	#endif
	// Use this for initialization
	IEnumerator Start ()
	{
		velocityVariation = Random.Range (0.8f, 1.2f);
		animator = GetComponentInChildren<Animator> ();
		cohesionSkipCount = Random.Range (0, COHESION_SKIPS) + COHESION_SKIPS; //Force update on first frame
		separationSkipCount = Random.Range (0, SEPARATION_SKIPS) + SEPARATION_SKIPS; //Force update on first frame
		alignmentSkipCount = Random.Range (0, ALIGNMENT_SKIPS) + ALIGNMENT_SKIPS; //Force update on first frame

		if (config.cohesionSearchRadius < config.separationSearchRadius || config.cohesionSearchRadius < config.alignmentSearchRadius) {
			config.cohesionSearchRadius = Mathf.Max (config.separationSearchRadius, config.alignmentSearchRadius) + 1;
			Debug.LogWarning ("Cohesion search radius must always be higher than alignment and separation");
		}

		if (animator != null) {
			var state = animator.GetCurrentAnimatorStateInfo (0);
			animator.Play (state.shortNameHash, 0, Random.value * state.length);
		}

		velocity = Random.insideUnitSphere.normalized;
		velocity.y = 0;
		velocity = velocity.normalized * config.maxVelocity;

		position = transform.position;


		cohesionNeighbours = 	new List<BirdAgent> ();
		separationNeighbours = 	new List<BirdAgent>();
		alignmentNeighbours =  	new List<BirdAgent>();

		if (config.searchInterval > 0) {
			float nextCycle = Random.Range (config.searchInterval, config.searchInterval * 2);
			do {
//				cohesionNeighbours = World.instance.GetNeighbours (this, config.cohesionSearchRadius);
//				if (config.separationSearchRadius > config.alignmentSearchRadius) {
//					separationNeighbours = World.instance.GetNeighbours (this, cohesionNeighbours, config.separationSearchRadius);//Reduce search scope to boost performance
//					alignmentNeighbours = World.instance.GetNeighbours (this, separationNeighbours, config.alignmentSearchRadius);//Reduce search scope to boost performance
//				} else {
//					alignmentNeighbours = World.instance.GetNeighbours (this, cohesionNeighbours, config.alignmentSearchRadius);//Reduce search scope to boost performance
//					separationNeighbours = World.instance.GetNeighbours (this, alignmentNeighbours, config.separationSearchRadius);//Reduce search scope to boost performance
//				}
				World.instance.GetAllNeighbours(this, config, ref cohesionNeighbours, ref alignmentNeighbours, ref separationNeighbours);
				yield return new WaitForSeconds (nextCycle);
				nextCycle = config.searchInterval;
			} while (true);
		}
	}

	public Vector3 debugVector;
	// Update is called once per frame
	void Update ()
	{
		acceleration = alive ? Vector3.ClampMagnitude (CombineFlockBehaviour (), config.maxAcceleration) : Vector3.down * 10;
       
		velocity += acceleration * Time.deltaTime;
		velocity = Vector3.ClampMagnitude (velocity, config.maxVelocity * velocityVariation);

		position += velocity * Time.deltaTime;
		transform.position = position;
		transform.forward = velocity;

		if (animator != null) {
			Vector3 directionVector = transform.InverseTransformPoint (position + acceleration).normalized;
			if (directionVector.z < 0) {
				if (Mathf.Abs (directionVector.x) > Mathf.Abs (directionVector.y)) {
					directionVector.x = 1 * Mathf.Sign (directionVector.x);
				} else {
					directionVector.y = 1 * Mathf.Sign (directionVector.y);
				}
				directionVector = Vector3.zero;
			}

			if (animVector == Vector3.zero) {
				animVector = directionVector;
			}

			animVector = Vector3.MoveTowards (animVector, directionVector, 4 * Time.deltaTime);

			animator.SetFloat ("DirectionX", animVector.x * 2);
			animator.SetFloat ("DirectionY", animVector.y * 2);
		}
	}

	public void SetWatchedBirds (List<BirdAgent> watched)
	{
		watchedBirds = watched;
	}

	public void DestroyBody ()
	{
		animator = null;
		foreach (Transform t in transform) {
			Destroy (t.gameObject);
		}
	}

	Vector3 CalculateCohesion ()
	{
		if (cohesionNeighbours.Count == 0) {
			cohesionSkipCount = COHESION_SKIPS;
			return Vector3.zero;
		}

		if (cohesionSkipCount++ >= COHESION_SKIPS) {
			cohesionSkipCount -= COHESION_SKIPS;
		} else {
			return cohesionCache;
		}

		Vector3 centerOfMass = Vector3.zero;
		for (var i = 0; i < cohesionNeighbours.Count; i++) {
			centerOfMass += cohesionNeighbours [i].position;
		}

		centerOfMass /= cohesionNeighbours.Count;

		cohesionCache = (centerOfMass - position).normalized;
		return cohesionCache;
	}

	Vector3 CalculateSeparation ()
	{
		if (separationNeighbours.Count == 0) {
			separationSkipCount = SEPARATION_SKIPS;
			return Vector3.zero;
		}

		if (separationSkipCount++ >= SEPARATION_SKIPS) {
			separationSkipCount -= SEPARATION_SKIPS;
		} else {
			return separationCache;
		}

		Vector3 repulsionForce = Vector3.zero;
		for (var i = 0; i < separationNeighbours.Count; i++) {
			Vector3 proximity = position - separationNeighbours [i].position;
			repulsionForce += proximity.normalized / proximity.magnitude;
		}

		separationCache = repulsionForce.normalized;
		return separationCache;
	}

	Vector3 CalculateAlignment ()
	{
		if (alignmentNeighbours.Count == 0) {
			alignmentSkipCount = ALIGNMENT_SKIPS;
			return Vector3.zero;
		}

		if (alignmentSkipCount++ >= ALIGNMENT_SKIPS) {
			alignmentSkipCount -= ALIGNMENT_SKIPS;
		} else {
			return alignmentCache;
		}

		Vector3 alignmentProduct = Vector3.zero;

		for (var i = 0; i < alignmentNeighbours.Count; i++) {
			alignmentProduct += alignmentNeighbours [i].velocity;
		}

		alignmentCache = alignmentProduct.normalized;
		return alignmentCache;
	}

	Vector3 CalculateAltitude ()
	{
		Vector3 v = Vector3.zero;
		if (position.y > config.maxAcceptableAltitude) {//Adjust for altitude;
			return Vector3.down * (position.y - config.maxAcceptableAltitude);
		}

		if (position.y < config.confortableAltitude) {
			return Vector3.up;
		} else {
			return Vector3.down;
		}
	}
	#if UNITY_EDITOR
	void OnDrawGizmos ()
	{
		if (debug) {
			if (cohesionNeighbours != null) { 
				Gizmos.color = Color.green;
				foreach (var c in cohesionNeighbours) {
					Gizmos.DrawLine (transform.position, c.transform.position);
				}
			}
			Gizmos.color = Color.red;
			Gizmos.DrawLine (transform.position, transform.position + acceleration.normalized * 3);
		}
	}
	#endif
	Vector3 CalculateCollision ()
	{
		Vector3 v = Vector3.zero;
		RaycastHit hitInfo;
		if (Physics.Raycast (position, transform.forward, out hitInfo, config.collisionRaycastDistance)) {
			float dist = Vector3.Distance (hitInfo.point, position);
			float mult = 1;
			mult = Mathf.Lerp (1, 25, Mathf.InverseLerp (config.collisionRaycastDistance, 1, dist));
			return Vector3.up * mult;
		}
		return v;
	}

	Vector3 CalculateBoundaries ()
	{
		Vector3 center = new Vector3 (0, position.y, 0);
		float distance = Vector3.Distance (position, center);
		if (distance < World.instance.boundariesRadius) {
			return Vector3.zero;
		}
		return (center - position).normalized * (distance - World.instance.boundariesRadius);
	}

	Vector3 CalculateWatchedBirds ()
	{
		if (watchedBirds == null || watchedBirds.Count == 0)
			return Vector3.zero;
		Vector3 center = Vector3.zero;
		int count = 0;
		foreach (var bird in watchedBirds) {
			try {
				if (Vector3.Distance (position, bird.position) <= config.watchedBirdLookRadius) {
					count++;
					center += bird.position;
				}
			} catch {
				watchedBirds.Remove (bird);
				break;
			}
		}
		if (count > 0) { 
			center /= count;
			return (center - position).normalized;
		}

		return Vector3.zero;
	}

	Vector3 CombineFlockBehaviour ()
	{
		Vector3 result = Vector3.zero;

		result += CalculateCohesion () * config.cohesionForce;
		result += CalculateSeparation () * config.separationForce;
		result += CalculateAlignment () * config.alignmentForce;
		result += CalculateAltitude () * config.altitudeAdjustForce;
		result += CalculateCollision () * config.avoidObstacleForce;
		result += CalculateBoundaries () * config.boundariesAdjustForce;
		result += CalculateWatchedBirds () * config.watchedBirdForce;
		return result;
	}
}
