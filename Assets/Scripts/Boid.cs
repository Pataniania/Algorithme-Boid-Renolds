using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Boid : MonoBehaviour
{
    public float VisionRange = 10f; 
    
    public List<Boid> GetNeighbors(List<Boid> allBoids)
    {
        List<Boid> neighbors = new List<Boid>();

        foreach (Boid other in allBoids)
        {
            if (other == this ) continue;

            float distance = Vector3.Distance(this.transform.position, other.transform.position);

            if (distance < VisionRange)
            {
                neighbors.Add(other);
            }
        }

        return neighbors; // POUR LE GETNEIGHBORS SINON CA BUG || ERREUR CS0161
    }

}
