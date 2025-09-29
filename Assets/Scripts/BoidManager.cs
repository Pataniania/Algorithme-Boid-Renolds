using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{

    [SerializeField] private int boidAmount = 10;
    [SerializeField] private Boids boidPrefab;

    private List<Boids> _boidList;

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

            _tempBoid.transform.position = _tempBoid.transform.position + _tempBoid.BoidVelocity;

            for (int j = i + 1; j < _boidList.Count; ++j)
            {
                float _tempDistance = Vector3.Distance(_tempBoid.transform.position, _boidList[j].transform.position);

                if (_tempDistance <= _tempBoid.VisualRange)
                {
                    if (_tempBoid.SeparationTreshold <= _tempDistance)
                    {
                       _tempBoid.transform.position = _tempBoid.transform.position + _tempBoid.BoidVelocity * .01f;
                    }
                }
            }

        }
    }
}
