using UnityEngine;

public class Boids : MonoBehaviour
{
    [SerializeField] private float visualRange = 20;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
}
