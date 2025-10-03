using System;
using System.Collections.Generic;
using UnityEngine;

public enum BoidProfiles
{
    BASE,
    SLOW,
    HERATIC,
    POTOFGLUE,
    LEADER
}

public class Boid : MonoBehaviour
{
    // === Configuration ===
    [SerializeField] private BoidProfiles profile = BoidProfiles.BASE;

    [Header("Materials")]
    [SerializeField] private Material baseBoidMaterial;
    [SerializeField] private Material slowBoidMaterial;
    [SerializeField] private Material potoMaterial;
    [SerializeField] private Material heraticMaterial;
    [SerializeField] private Material leaderMaterial;

    [Header("Boid Behavior Parameters")]
    [SerializeField] private float visionRadius = 6f;
    [SerializeField] private float separationWeight = 1.5f;
    [SerializeField] private float alignmentWeight = 1.0f;
    [SerializeField] private float cohesionWeight = 1.0f;
    [SerializeField] private float centerOfMassWeight = 0.8f;

    [Header("Movement Constraints")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float maxForce = 0.5f;
    [SerializeField] private float randomTargetRadius = 10000f;

    [Header("Bounds Settings")]
    [SerializeField] private float bounds = 20f;
    [SerializeField] private float boundsForce = 10f;
    [SerializeField] private float boundsThreshold = 10f;

    // === Runtime State ===
    private Vector3 _velocity;
    private Vector3 _acceleration;
    private Vector3 _randomTargetPos;

    private static List<Boid> _boidList = new List<Boid>();
    private List<Boid> _neighborBuffer = new List<Boid>(100);
    private List<Boid> _neighbors;

    private static Boid _leaderBoid;
    private bool _hasBeenReached;



    // === Unity Lifecycle ===
    private void OnEnable() => _boidList.Add(this);
    private void OnDisable() => _boidList.Remove(this);

    private void Start()
    {
        InitializeBoid();
    }

    private void Update()
    {
        _acceleration = Vector3.zero;
        bounds = randomTargetRadius;
        _neighbors = GetNeighbors();

        ApplyForcesToBoid();
        UpdateBoidVelocity();
        UpdateLookDirection();

        if (profile == BoidProfiles.LEADER)
        {
            UpdateLeader();
            //UpdateRandomTarget();
        }

    }

    // === Initialization ===
    private void InitializeBoid()
    {
        _randomTargetPos = UnityEngine.Random.insideUnitSphere * randomTargetRadius;

        switch (profile)
        {
            case BoidProfiles.BASE:
                _velocity = UnityEngine.Random.insideUnitSphere * maxSpeed;
                GetComponent<Renderer>().material = baseBoidMaterial;
                break;

            case BoidProfiles.SLOW:
                visionRadius = 5.5f;
                separationWeight = 1.2f;
                alignmentWeight = 1.0f;
                cohesionWeight = 1.5f;
                centerOfMassWeight = 1.0f;
                maxSpeed = 2.5f;
                maxForce = 0.25f;
                boundsForce = 10f;
                GetComponent<Renderer>().material = slowBoidMaterial;
                break;


            case BoidProfiles.POTOFGLUE:
                visionRadius = 7.5f;
                separationWeight = 0.01f;
                alignmentWeight = 3.5f;
                cohesionWeight = 3.2f;
                centerOfMassWeight = 2.0f;
                maxSpeed = 6f;
                maxForce = 0.6f;
                boundsForce = 14f;
                _velocity = UnityEngine.Random.insideUnitSphere * maxSpeed;
                GetComponent<Renderer>().material = potoMaterial;
                break;

            case BoidProfiles.LEADER:
                _velocity = UnityEngine.Random.insideUnitSphere * maxSpeed;
                _leaderBoid = this;
                name = "Leader Boid";

                GetComponent<Renderer>().material = leaderMaterial;
                break;
        }
    }

    // === Main Boid Logic ===
    private void ApplyForcesToBoid()
    {
        GoTowardsLeader();
        _acceleration += CalculateFlockingForce(_neighbors);
        ApplyAdditionalForces();
    }

    private Vector3 CalculateFlockingForce(List<Boid> neighbors)
    {
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 separation = Separation(neighbors) * separationWeight;
        Vector3 alignment = Alignment(neighbors) * alignmentWeight;
        Vector3 cohesion = Cohesion(neighbors) * cohesionWeight;

        return separation + alignment + cohesion;
    }

    private void ApplyAdditionalForces()
    {
        _acceleration += KeepInBounds();
        _acceleration += ApplyCenterOfMassForce() * centerOfMassWeight;
        _acceleration = Vector3.ClampMagnitude(_acceleration, maxForce);
    }

    private void UpdateBoidVelocity()
    {
        _velocity += _acceleration * Time.deltaTime;
        _velocity = Vector3.ClampMagnitude(_velocity, maxSpeed);
        transform.position += _velocity * Time.deltaTime;
    }

    private void UpdateLookDirection()
    {
        if (_velocity.sqrMagnitude > 0.01f)
            transform.forward = _velocity.normalized;
    }

    private void UpdateLeader()
    {
        if (_leaderBoid == null) return;

        Vector3 toTarget = _randomTargetPos - transform.position;
        float distance = toTarget.magnitude;

        // Slow down as we get closer
        float slowingRadius = 5f;
        float speed = Mathf.Lerp(0, maxSpeed, distance / slowingRadius);
        speed = Mathf.Min(speed, maxSpeed);

        Vector3 desired = toTarget.normalized * speed;
        Vector3 steer = Vector3.ClampMagnitude(desired - _velocity, maxForce);

        _acceleration += steer;

        _randomTargetPos = UnityEngine.Random.insideUnitSphere * randomTargetRadius;

    }

    private void GoTowardsLeader()
    {
        if (_leaderBoid != null && profile != BoidProfiles.LEADER)
        {
            Vector3 toLeader = _leaderBoid.transform.position - transform.position;
            float distance = toLeader.magnitude;

            if (distance > 3f)
            {
                Vector3 leaderForce = Vector3.ClampMagnitude(toLeader.normalized * maxSpeed - _velocity, maxForce);
                _acceleration += leaderForce * 3f;
            }
        }
    }

    // === Neighbor and Steering Behaviors ===
    private List<Boid> GetNeighbors()
    {
        _neighborBuffer.Clear();

        foreach (Boid other in _boidList)
        {
            if (other == this) continue;

            float distSqr = (other.transform.position - transform.position).sqrMagnitude;
            if (distSqr < visionRadius * visionRadius)
            {
                _neighborBuffer.Add(other);
            }
        }

        return _neighborBuffer;
    }

    private Vector3 Separation(List<Boid> neighbors)
    {
        Vector3 steer = Vector3.zero;

        foreach (Boid other in neighbors)
        {
            Vector3 diff = transform.position - other.transform.position;
            float dist = diff.magnitude;

            if (dist > 0)
                steer += diff.normalized / dist;
        }

        return steer;
    }

    private Vector3 Alignment(List<Boid> neighbors)
    {
        return SteerTowardsAverage(neighbors, b => b._velocity, _velocity);
    }

    private Vector3 Cohesion(List<Boid> neighbors)
    {
        return SteerTowardsAverage(neighbors, b => b.transform.position, transform.position);
    }

    private Vector3 ApplyCenterOfMassForce()
    {
        return SteerTowardsAverage(_boidList, b => b.transform.position, transform.position);
    }

    private Vector3 SteerTowardsAverage(List<Boid> group, Func<Boid, Vector3> selector, Vector3 reference)
    {
        if (group.Count == 0) return Vector3.zero;

        Vector3 sum = Vector3.zero;
        foreach (Boid b in group)
            sum += selector(b);

        Vector3 average = sum / group.Count;
        return average - reference;
    }

    private Vector3 KeepInBounds()
    {
        Vector3 force = Vector3.zero;

        if (Mathf.Abs(transform.position.x) > bounds - boundsThreshold)
            force.x = -Mathf.Sign(transform.position.x) * boundsForce;

        if (Mathf.Abs(transform.position.y) > bounds - boundsThreshold)
            force.y = -Mathf.Sign(transform.position.y) * boundsForce;

        if (Mathf.Abs(transform.position.z) > bounds - boundsThreshold)
            force.z = -Mathf.Sign(transform.position.z) * boundsForce;

        return force;
    }

    // === Debug Gizmos ===
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + _velocity);

        // Optional: Draw the full random range boundary (gray)
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(Vector3.zero, randomTargetRadius);
    }

    // === Properties ===
    public BoidProfiles BoidProfiles
    {
        get => profile;
        set => profile = value;
    }
}
