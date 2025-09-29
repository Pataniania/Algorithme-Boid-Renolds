using UnityEngine;

public class Boids : MonoBehaviour
{
    [SerializeField] private float visualRange = 40;
    [SerializeField] private float separationTreshold = 10;

    private Vector3 _velocity;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _velocity = new Vector3(
            Random.Range(0f, .01f),
            Random.Range(0f, .01f),
            Random.Range(0f, .01f)
            );
        transform.position = new Vector3(
            Random.Range(0f, 100f),
            Random.Range(0f, 100f),
            Random.Range(0f, 100f)
            );
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDrawGizmos()
    {
        // Set the color with custom alpha.
        Gizmos.color = new Color(1f, 0f, 0f, .2f); // Red with custom alpha
        Gizmos.DrawWireSphere(transform.position, visualRange);
    }

    public float VisualRange
    {
        get { return visualRange; }
    }
    public float SeparationTreshold
    {
        get { return separationTreshold; }
    }

    public Vector3 BoidVelocity
    {
        get { return _velocity; }
    }
}
