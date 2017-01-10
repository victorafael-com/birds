using UnityEngine;
using System.Collections;

public class BirdAgentConfig : MonoBehaviour {
    public GameObject prefab;

    [Header("Specimes Behaviour")]
    public float confortableAltitude;
    public float maxAcceptableAltitude;
    public BirdType type;

    [Header("Movement")]
    public float maxAcceleration;
    public float maxVelocity;

    [Header("Search")]
    public float searchIntervalMin;
    public float searchIntervalMax;
    [Space]
    public float cohesionSearchRadius = 15;
    public float separationSearchRadius = 4;
    public float alignmentSearchRadius = 6;
    public float collisionRaycastDistance = 10;
    public float watchedBirdLookRadius = 8;

    [Header("Desire Force")]
    public float cohesionForce = 4;
    public float separationForce = 2;
    public float alignmentForce = 4;
    public float avoidObstacleForce = 10;
    public float altitudeAdjustForce = 0.5f;
    public float boundariesAdjustForce = 0.5f;
    public float watchedBirdForce = -10;

}
