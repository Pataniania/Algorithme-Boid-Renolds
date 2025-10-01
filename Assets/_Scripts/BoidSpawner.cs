using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    [SerializeField] private GameObject boidPrefab;
    [SerializeField] private int boidCount = 50;
    [SerializeField] private float spawnRadius = 10f;



    void Start()
    {
        SpawnBoid();
    }


    private void SpawnBoid()
    {
        for (int i = 0; i < boidCount; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            Instantiate(boidPrefab, pos, Quaternion.identity);
        }
    }
}
