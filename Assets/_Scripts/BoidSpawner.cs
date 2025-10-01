using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    [SerializeField] private GameObject boidPrefab;
    [SerializeField] private int boidCount = 50;
    [SerializeField] private float spawnRadius = 10f;

    [SerializeField][Range(0f, 1f)] private float baseProfileSpawnRate = .9f;

    void Start()
    {
        SpawnBoid();
    }

    void SpawnBoid()
    {
        for (int i = 0; i < boidCount; i++)
        {
            //[Note(IVANIA)] A bit racist

            Boid boid = boidPrefab.GetComponent<Boid>();
            boid.BoidProfiles = UnityEngine.Random.value < baseProfileSpawnRate
                ? BoidProfiles.BASE
                : BoidProfiles.SLOW;

            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            Instantiate(boidPrefab, pos, Quaternion.identity);
        }
    }

    public int BoidCount
    {
        get { return boidCount; }
    }
}
