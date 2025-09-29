using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public float visionRadius = 5f;

    public float separationWeight = 1.5f;
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 1.0f;
    public float centerOfMassWeight = 1.0f;

    public float maxSpeed = 5f;
    public float maxForce = 0.5f;

    public float bounds = 10f;
    public float boundsForce = 10f; // force applique quand on se rapproche du bord
    public float boundsThreshold = 10f; //distance a partir du bord pour declencher la force

    private Vector3 velocity;
    private Vector3 acceleration;

    // liste globale de tous les boids
    public static List<Boid> allBoids = new List<Boid>();

    void OnEnable() => allBoids.Add(this);
    void OnDisable() => allBoids.Remove(this);

    void Start()
    {
        velocity = Random.insideUnitSphere * maxSpeed;
    }

    void Update()
    {
        acceleration = Vector3.zero;

        List<Boid> neighbors = GetNeighbors();

        if (neighbors.Count > 0)
        {
            Vector3 sep = Separation(neighbors) * separationWeight;
            Vector3 ali = Alignment(neighbors) * alignmentWeight;
            Vector3 coh = Cohesion(neighbors) * cohesionWeight;

            acceleration += sep + ali + coh;
        }

        // add de la force de rappel pour rester in bound
        acceleration += KeepInBounds();

        // force du centre de masse
        acceleration += CenterOfMassForce() * centerOfMassWeight;

        // clamp la force finale
        acceleration = Vector3.ClampMagnitude(acceleration, maxForce);

        // update velocity + pos
        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;

        if (velocity.sqrMagnitude > 0.01f)
            transform.forward = velocity.normalized;
    }

    List<Boid> GetNeighbors()
    {
        List<Boid> neighbors = new List<Boid>();
        foreach (Boid other in allBoids)
        {
            if (other == this) continue;

            float dist = (other.transform.position - transform.position).magnitude;
            if (dist < visionRadius)
            {
                neighbors.Add(other);
            }
        }
        return neighbors;
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
        Vector3 avgVelocity = Vector3.zero;
        foreach (Boid other in neighbors)
        {
            avgVelocity += other.velocity;
        }
        avgVelocity /= neighbors.Count;
        return avgVelocity - velocity; // steering vers la moyenne
    }

    Vector3 Cohesion(List<Boid> neighbors)
    {
        Vector3 center = Vector3.zero;
        foreach (Boid other in neighbors)
        {
            center += other.transform.position;
        }
        center /= neighbors.Count;
        return (center - transform.position);
    }

    Vector3 KeepInBounds()
    {
        Vector3 force = Vector3.zero;

        if (Mathf.Abs(transform.position.x) < bounds - boundsThreshold)
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

    Vector3 CenterOfMassForce()
    {
        if (allBoids.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 com = Vector3.zero;
        foreach (Boid b in allBoids)
        {
            com += b.transform.position;
        }
        com /= allBoids.Count;

        return (com - transform.position);
    }
}
