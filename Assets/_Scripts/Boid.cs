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

    [SerializeField] private BoidProfiles profile = BoidProfiles.BASE;

    [SerializeField] private Material baseBoidMaterial;
    [SerializeField] private Material slowBoidMaterial;
    [SerializeField] private Material potoMaterial;
    [SerializeField] private Material heraticMaterial;
    [SerializeField] private Material leaderMaterial;

    [SerializeField] private float visionRadius = 6f;

    [SerializeField] private float separationWeight = 1.5f;
    [SerializeField] private float alignmentWeight = 1.0f;
    [SerializeField] private float cohesionWeight = 1.0f;
    [SerializeField] private float centerOfMassWeight = 0.8f;

    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float maxForce = 0.5f;

    [SerializeField] private float bounds = 10f;
    //Force applied to the boid when it's near the border
    [SerializeField] private float boundsForce = 10f;
    //Distance from the border where the force is applied
    [SerializeField] private float boundsThreshold = 10f;

    private Vector3 _velocity;
    private Vector3 _acceleration;

    private static List<Boid> _boidList = new List<Boid>();

    private List<Boid> _neighborBuffer = new List<Boid>(100);
    private List<Boid> _neighbors;

    private Material _material;
    private static Boid _leaderBoid;


    void OnEnable() => _boidList.Add(this);
    void OnDisable() => _boidList.Remove(this);

    void Start()
    {
        
        
        switch (profile)
        {
            case BoidProfiles.BASE:

                _velocity = new Vector3(
                    UnityEngine.Random.Range(1f, maxSpeed),
                    UnityEngine.Random.Range(1f, maxSpeed),
                    UnityEngine.Random.Range(1f, maxSpeed)
                    );

                GetComponent<MeshRenderer>().material = baseBoidMaterial;
                break;

            case BoidProfiles.SLOW:

                visionRadius = 5.5f;
                cohesionWeight = 1.2f;
                maxSpeed = 2.5f;
                maxForce = 0.3f; visionRadius = 5.5f;

                separationWeight = 1.2f;
                alignmentWeight = 1.0f;
                cohesionWeight = 1.5f;
                centerOfMassWeight = 1.0f;

                maxSpeed = 2.5f;
                maxForce = 0.25f;

                boundsForce = 10f;

                GetComponent<MeshRenderer>().material = slowBoidMaterial;

                break;

            case BoidProfiles.HERATIC:
                _velocity = new Vector3(
                    UnityEngine.Random.Range(2f, maxSpeed),
                    UnityEngine.Random.Range(2f, maxSpeed),
                    UnityEngine.Random.Range(2f, maxSpeed)
                );

                visionRadius = 3.5f;
                separationWeight = 0.1f;
                alignmentWeight = 0.1f;
                cohesionWeight = 0.1f;
                centerOfMassWeight = 0f;

                maxSpeed = 6.5f;
                maxForce = 1.2f;

                boundsForce = 4f;

                GetComponent<Renderer>().material = heraticMaterial;
                break;

            case BoidProfiles.POTOFGLUE:

                _velocity = new Vector3(
                    UnityEngine.Random.Range(1f, maxSpeed),
                    UnityEngine.Random.Range(1f, maxSpeed),
                    UnityEngine.Random.Range(1f, maxSpeed)
                    );

                maxSpeed = 5f;

                visionRadius = 7.5f;

                separationWeight = 0.1f;
                alignmentWeight = 1.5f;
                cohesionWeight = 1.2f;
                centerOfMassWeight = 2.0f;

                maxForce = 0.6f;

                boundsForce = 14f;

                GetComponent<Renderer>().material = potoMaterial;
                break;

            case BoidProfiles.LEADER:

                _velocity = new Vector3(
                    UnityEngine.Random.Range(1f, maxSpeed),
                    UnityEngine.Random.Range(1f, maxSpeed),
                    UnityEngine.Random.Range(1f, maxSpeed)
                    );

                _leaderBoid = this;
                _leaderBoid.name = "Leader Boid";

                GetComponent<Renderer>().material = leaderMaterial;

                break;
        }
    }

    void Update()
    {
        _acceleration = Vector3.zero;

        _neighbors = GetNeighbors();

        ApplyForcesToBoid();

        //Update the boid velocity and clamp it
        UpdateBoidVelocity();

        if(profile == BoidProfiles.LEADER)
        {
            UpdateLeader();
        }

        UpdateLookDir();
    }
    void UpdateLeader()
    {
        Vector3 targetPosition;

        if(_leaderBoid != null)
        {
            targetPosition = Vector3.zero;
        }
        else
        {
            targetPosition = ApplyCenterOfMassForce();
        }

        Vector3  desired = targetPosition - transform.position;
        desired = desired.normalized * maxSpeed;

        Vector3 steer = desired - _velocity;
        steer = Vector3.ClampMagnitude(steer, maxForce);

        _acceleration = steer;

        UpdateBoidVelocity();

    }
    void UpdateLookDir()
    {
        if (_velocity.sqrMagnitude > 0.01f)
            transform.forward = _velocity.normalized;
    }

    List<Boid> GetNeighbors()
    {
        _neighborBuffer.Clear();

        foreach (Boid other in _boidList)
        {
            if (other == this) continue;

            float dist = (other.transform.position - transform.position).sqrMagnitude;

            if (dist < (visionRadius * visionRadius))
            {
                _neighborBuffer.Add(other);
            }
        }
        return _neighborBuffer;
    }

    Vector3 Separation(List<Boid> neighbors)
    {
        Vector3 steer = Vector3.zero;

        foreach (Boid other in neighbors)
        {
            Vector3 diff = transform.position - other.transform.position;

            float dist = diff.magnitude;

            if (dist > 0) 
                steer += diff.normalized / dist; // plus proche = push plus fort
        }
        return steer;
    }

    Vector3 Alignment(List<Boid> neighbors)
    {
        return SteerTowardsAverage(neighbors, b => b._velocity, _velocity);
    }

    Vector3 Cohesion(List<Boid> neighbors)
    {
        return SteerTowardsAverage(neighbors, b => b.transform.position, transform.position);
    }

    Vector3 ApplyCenterOfMassForce()
    {
       return SteerTowardsAverage(_boidList, b => b.transform.position, transform.position);
    }

    Vector3 KeepInBounds()
    {
        Vector3 force = Vector3.zero;

        if (Mathf.Abs(transform.position.x) > bounds - boundsThreshold)
        {
            force.x = -Mathf.Sign(transform.position.x) * boundsForce;
        }

        if (Mathf.Abs(transform.position.y) > bounds - boundsThreshold)
        {
            force.y = -Mathf.Sign(transform.position.y) * boundsForce;
        }

        if (Mathf.Abs(transform.position.z) > bounds - boundsThreshold)
        {
            force.z = -Mathf.Sign(transform.position.z) * boundsForce;
        }

        return force;
    }

    Vector3 SteerTowardsAverage(List<Boid> neighbors, Func<Boid, Vector3> selector, Vector3 currentPosition)
    {
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 average = Vector3.zero;

        foreach (Boid b in neighbors)
        {
            average += selector(b);
        }
        average /= neighbors.Count;

        return average - currentPosition;
    }

    private void ApplyForcesToBoid()
    {

        if (_leaderBoid != null && profile != BoidProfiles.LEADER)
        {
            // Follow the leader's position
            Vector3 toLeader = _leaderBoid.transform.position - transform.position;

            float distanceToLeader = toLeader.magnitude;
            float leaderFollowWeight = 5.0f;

            // If far, prioritize leader-following more
            if (distanceToLeader > 3f)
            {
                Vector3 leaderForce = toLeader.normalized * maxSpeed - _velocity;
                leaderForce = Vector3.ClampMagnitude(leaderForce, maxForce);
                _acceleration += leaderForce * leaderFollowWeight;
            }
        }
        if (_neighbors.Count > 0)
        {
            Vector3 _separation = Separation(_neighbors) * separationWeight;
            Vector3 _alignment = Alignment(_neighbors) * alignmentWeight;
            Vector3 _cohesion = Cohesion(_neighbors) * cohesionWeight;

            _acceleration += _separation + _alignment + _cohesion;
        }

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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + _velocity);
    }

    public BoidProfiles BoidProfiles
    {
        get { return profile; }
        set { profile = value; }
    }
}
