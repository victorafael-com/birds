using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BirdAgent : MonoBehaviour {
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

    public bool alive = true;

    private float velocityVariation;
#if UNITY_EDITOR
    public bool debug;
#endif
    // Use this for initialization
    IEnumerator Start () {
        velocityVariation = Random.Range(0.95f, 1.05f);
        if(config.searchIntervalMin > 0) { 
            do {
                cohesionNeighbours = World.instance.GetNeighbours(this, config.cohesionSearchRadius);
                separationNeighbours = World.instance.GetNeighbours(this, config.separationSearchRadius);
                alignmentNeighbours = World.instance.GetNeighbours(this, config.alignmentSearchRadius);
                yield return new WaitForSeconds(Random.Range(config.searchIntervalMin, config.searchIntervalMax));
            } while (true);
        } else {
            cohesionNeighbours = new List<BirdAgent>();
            separationNeighbours = new List<BirdAgent>();
            alignmentNeighbours = new List<BirdAgent>();
            velocity = Random.insideUnitSphere.normalized;
            velocity.y = 0;
            velocity = velocity.normalized * config.maxVelocity;
        }
    }
	
	// Update is called once per frame
	void Update () {
        acceleration = alive ? Vector3.ClampMagnitude( CombineFlockBehaviour() , config.maxAcceleration) : Vector3.down * 10;
       
        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, config.maxVelocity * velocityVariation);

        transform.position += velocity * Time.deltaTime;
        transform.forward = velocity;
	}

    public void SetWatchedBirds(List<BirdAgent> watched) {
        watchedBirds = watched;
    }

    Vector3 CalculateCohesion() {
        if(cohesionNeighbours.Count == 0)
            return Vector3.zero;

        Vector3 centerOfMass = Vector3.zero;
        foreach (var a in cohesionNeighbours)
            centerOfMass += a.transform.position;

        centerOfMass /= cohesionNeighbours.Count;

        return (centerOfMass - transform.position).normalized;
    }
    Vector3 CalculateSeparation() {
        if (separationNeighbours.Count == 0)
            return Vector3.zero;

        Vector3 repulsionForce = Vector3.zero;
        foreach(var neighbour in separationNeighbours) {
            Vector3 proximity = transform.position - neighbour.transform.position;
            if(proximity.magnitude > 0) {
                repulsionForce += proximity.normalized / proximity.magnitude;
            }
        }

        return repulsionForce.normalized;
    }
    Vector3 CalculateAlignment() {
        if (alignmentNeighbours.Count == 0)
            return Vector3.zero;

        Vector3 alignmentProduct = Vector3.zero;

        foreach(var agent in alignmentNeighbours) {
            alignmentProduct += agent.velocity;
        }

        return alignmentProduct.normalized;
    }

    Vector3 CalculateAltitude() {
        Vector3 v = Vector3.zero ;
        if (transform.position.y > config.maxAcceptableAltitude) {//Adjust for altitude;
            return Vector3.down * (transform.position.y - config.maxAcceptableAltitude);
        }

        if(transform.position.y < config.confortableAltitude) {
            return Vector3.up;
        } else {
            return Vector3.down;
        }
    }
#if UNITY_EDITOR
    void OnDrawGizmos() {
        if (debug) {
            if(cohesionNeighbours != null) { 
                Gizmos.color = Color.green;
                foreach(var c in cohesionNeighbours) {
                    Gizmos.DrawLine(transform.position, c.transform.position);
                }
            }
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + acceleration.normalized*3);
        }
    }
#endif
    Vector3 CalculateCollision() {
        Vector3 v = Vector3.zero;
        RaycastHit hitInfo;
        if(Physics.Raycast(transform.position, transform.forward, out hitInfo, config.collisionRaycastDistance)) {
            float dist = Vector3.Distance(hitInfo.point, transform.position);
            float mult = 1;
            mult = Mathf.Lerp(1, 25, Mathf.InverseLerp(config.collisionRaycastDistance, 1, dist));
            return Vector3.up * mult;
        }
        return v;
    }
    Vector3 CalculateBoundaries() {
        Vector3 center = new Vector3(0, transform.position.y, 0);
        float distance = Vector3.Distance(transform.position, center);
        if (distance < World.instance.boundariesRadius) {
            return Vector3.zero;
        }
        return (center - transform.position).normalized * (distance - World.instance.boundariesRadius);
    }
    Vector3 CalculateWatchedBirds() {
        if (watchedBirds == null || watchedBirds.Count == 0) return Vector3.zero;
        Vector3 center = Vector3.zero;
        int count = 0;
        foreach(var bird in watchedBirds) {
            if (bird == null) {
                watchedBirds.Remove(bird);
                break;
            } else { 
                if(Vector3.Distance(transform.position, bird.transform.position) <= config.watchedBirdLookRadius) {
                    count++;
                    center += bird.transform.position;
                }
            }
        }
        if(count > 0) { 
            center /= count;
            return (center - transform.position).normalized;
        }

        return Vector3.zero;
    }
    Vector3 CombineFlockBehaviour() {
        Vector3 result = Vector3.zero;

        result += CalculateCohesion() * config.cohesionForce;
        result += CalculateSeparation() * config.separationForce;
        result += CalculateAlignment() * config.alignmentForce;
        result += CalculateAltitude() * config.altitudeAdjustForce;
        result += CalculateCollision() * config.avoidObstacleForce;
        result += CalculateBoundaries() * config.boundariesAdjustForce;
        result += CalculateWatchedBirds() * config.watchedBirdForce;
        return result;
    }
}
