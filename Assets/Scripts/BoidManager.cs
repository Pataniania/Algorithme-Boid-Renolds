using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    [SerializeField] private int boidAmount = 10;
    [SerializeField] private Boids boidPrefab;
    [SerializeField] private Boids leaderBoidPrefab;

    private List<Boids> _boidList;
    private Vector3 _averagePosition;
    private Vector3 _averageVelocity;

    void Start()
    {
        _boidList = new List<Boids>();

        for (int i = 0; i < boidAmount; i++)
        {
            Boids newBoid = Instantiate(boidPrefab);
            _boidList.Add(newBoid);
        }
        leaderBoidPrefab = Instantiate(leaderBoidPrefab);
    }

    void Update()
    {
        // Calculate average position and velocity once per frame
        _averagePosition = new Vector3(
            _boidList.Average(p => p.transform.position.x),
            _boidList.Average(p => p.transform.position.y),
            _boidList.Average(p => p.transform.position.z)
        );

        _averageVelocity = new Vector3(
            _boidList.Average(p => p.BoidVelocity.x),
            _boidList.Average(p => p.BoidVelocity.y),
            _boidList.Average(p => p.BoidVelocity.z)
        );

        for (int i = 0; i < _boidList.Count; i++)
        {
            Boids _tempBoid = _boidList[i];

            _tempBoid.transform.position += _tempBoid.BoidVelocity;

            for (int j = i + 1; j < _boidList.Count; ++j)
            {
                float _tempDistance = Vector3.Distance(_tempBoid.transform.position, _boidList[j].transform.position);

                if (_tempDistance <= _tempBoid.VisualRange)
                {
                    if (_tempBoid.SeparationTreshold <= _tempDistance)
                    {
                        _tempBoid.transform.position += _tempBoid.BoidVelocity;
                    }

                    if (_tempBoid.transform.position != _averagePosition)
                    {
                        transform.position = Vector3.Lerp(_tempBoid.transform.position, _averagePosition, 1f);
                    }

                    if (_tempBoid.BoidVelocity != _averageVelocity)
                    {
                        
                        _tempBoid.BoidVelocity = Vector3.Lerp(_tempBoid.BoidVelocity, _averageVelocity, 1f);
                        Debug.Log(_averageVelocity);
                        Debug.Log(_boidList[i].BoidVelocity);
                    }

                    Vector3 direction = (_averagePosition - leaderBoidPrefab.transform.position).normalized;
                    leaderBoidPrefab.transform.position += direction * 2f * Time.deltaTime;
                }
            }
        }
    }

    // Draw velocity and average velocity vectors in the scene view
    private void OnDrawGizmos()
    {
        if (_boidList == null)
            return;

        // Draw velocity for each boid in green
        Gizmos.color = Color.green;
        foreach (var boid in _boidList)
        {
            if (boid != null)
                Gizmos.DrawLine(boid.transform.position, boid.transform.position + boid.BoidVelocity);
        }

        // Draw average velocity in blue at the average position
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_averagePosition, _averagePosition + _averageVelocity);

        // Draw average position as a yellow sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_averagePosition, 0.2f);

        // Draw leader position as a red sphere
        if (leaderBoidPrefab != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(leaderBoidPrefab.transform.position, 0.3f);
        }
    }
}
