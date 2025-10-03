using System;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    [SerializeField] private GameObject boidPrefab;
    [SerializeField] private int boidCount = 50;
    [SerializeField] private float spawnRadius = 10f;

    private void Start()
    {
        SpawnBoids();
    }

    private void SpawnBoids()
    {
        BoidProfiles[] allProfiles = (BoidProfiles[])Enum.GetValues(typeof(BoidProfiles));
        List<BoidProfiles> specialProfiles = new List<BoidProfiles>();

        foreach (BoidProfiles profile in allProfiles)
        {
            if (profile != BoidProfiles.BASE)
                specialProfiles.Add(profile);
        }

        for (int i = 0; i < boidCount; i++)
        {
            Vector3 spawnPos = transform.position + UnityEngine.Random.insideUnitSphere * spawnRadius;
            GameObject boidInstance = Instantiate(boidPrefab, spawnPos, Quaternion.identity);
            Boid boid = boidInstance.GetComponent<Boid>();

            // Assign special profiles to first N boids, rest are BASE
            boid.BoidProfiles = i < specialProfiles.Count ? specialProfiles[i] : BoidProfiles.BASE;
        }
    }

    public int BoidCount => boidCount;
}
