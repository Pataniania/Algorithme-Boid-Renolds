using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{

    [SerializeField] private int boidAmount = 10;
    [SerializeField] private Boids boidPrefab;

    private List<Boids> _boidList;
    private Boids _tempBoid;
    private float _tempDistance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _boidList = new List<Boids>();

        for (int i = 0; i < boidAmount; i++)
        {
            Boids newBoid = Instantiate(boidPrefab);
            _boidList.Add(newBoid);
        }

    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _boidList.Count; i++)
        {
            Boids _tempBoid = _boidList[i];
            Debug.Log("Current Boid position is " + _tempBoid.transform.position);

            for (int j = i + 1; j < _boidList.Count; ++j)
            {
                float _tempDistance = Vector3.Distance(_tempBoid.transform.position, _boidList[j].transform.position);

                Debug.Log("The distance between boid " + i + " and " + j + " is: " + _tempDistance);

                if (_tempDistance <= _tempBoid.VisualRange)
                {
                    //Debug.Log(_tempDistance);
                }
            }

        }
    }
}
