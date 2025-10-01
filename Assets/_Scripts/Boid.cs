using System;
using System.Collections.Generic;
using UnityEngine;
public enum BoidProfiles
{
    BASE,
    SLOW,
    LEADER
}
public class Boid : MonoBehaviour
{

    [SerializeField] private BoidProfiles profile = BoidProfiles.BASE;
    [SerializeField] private Material baseBoidMaterial;
    [SerializeField] private Material slowBoidMaterial;

    [SerializeField] private float visionRadius = 5f;

    [SerializeField] private float separationWeight = 1.5f;
    [SerializeField] private float alignmentWeight = 1.0f;
    [SerializeField] private float cohesionWeight = 1.0f;
    [SerializeField] private float centerOfMassWeight = 1.0f;

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

                visionRadius = 4.5f;
                cohesionWeight = 1.2f;
                maxSpeed = 2.5f;
                maxForce = 0.3f;

                GetComponent<MeshRenderer>().material = slowBoidMaterial;

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

        transform.position += _velocity * Time.deltaTime;

        UpdateLookDir();
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

            if (dist < visionRadius)
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
