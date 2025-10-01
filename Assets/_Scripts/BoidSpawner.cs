using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    [SerializeField] private GameObject boidPrefab;
    [SerializeField] private int boidCount = 50;
    [SerializeField] private float spawnRadius = 10f;

    [SerializeField][UnityEngine.Range(0f, 1f)] private float baseProfileSpawnRate = .9f;

    void Start()
    {
        SpawnBoid();
    }

    void SpawnBoid()
    {
        BoidProfiles[] allProfiles = (BoidProfiles[])Enum.GetValues(typeof(BoidProfiles));
        List<BoidProfiles> specialProfiles = new List<BoidProfiles>();

        foreach (BoidProfiles profile in allProfiles)
        { 
            if(profile != BoidProfiles.BASE)
            {
                specialProfiles.Add(profile);
            }
        }
        
        for (int i = 0; i < boidCount; i++)
        {
            
            Vector3 spawnPosition = transform.position + UnityEngine.Random.insideUnitSphere * spawnRadius;
            GameObject boidInstance = Instantiate(boidPrefab, spawnPosition, Quaternion.identity);

            Boid boid = boidInstance.GetComponent<Boid>();

            boid.BoidProfiles = i < specialProfiles.Count ? boid.BoidProfiles = specialProfiles[i] : BoidProfiles.BASE;
        }
    }

    public int BoidCount
    {
        get { return boidCount; }
    }
}
