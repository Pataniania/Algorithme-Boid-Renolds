using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    public GameObject boidPrefab;
    public int boidCount = 50;
    public float spawnRadius = 10f;

    void Start()
    {
        for (int i = 0; i < boidCount; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            Instantiate(boidPrefab, pos, Quaternion.identity);
        }
    }
}
