using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public int boidQuantity;

    public GameObject boidPrefab;

    public List<GameObject> boids;

    public List<Vector3> boidPositions;
    public List<Vector3> boidVelocity;
    public Vector3 averagePosition;

    public Vector3 minRange, maxRange;

    void Start()
    {
        StartCoroutine(Generateboids());
    }

    IEnumerator Generateboids()
    {
        Vector3 posTotal = Vector3.zero;
        for (int i = 0; i < boidQuantity; i++)
        {
            Vector3 startPos = new Vector3(Random.Range(minRange.x, maxRange.x), 
                                           Random.Range(minRange.y, maxRange.y), 
                                           Random.Range(minRange.z, maxRange.z));
            GameObject newboid = Instantiate(boidPrefab, startPos, Quaternion.identity);
            newboid.transform.position = startPos;
            boids.Add(newboid);
            boidPositions.Add(startPos);
            boidVelocity.Add(Vector3.zero);
            posTotal += startPos;
            yield return new WaitForEndOfFrame();
        }
        averagePosition = posTotal / boidQuantity;
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(UpdateBoids());
    }

    IEnumerator UpdateBoids()
    {
        Vector3 v1, v2, v3;

        for (int i = 0; i < boidQuantity; i++)
        {
            // update each boid
            // move boid closer to average position
            // base speed on how far the boid is from the average
            // update boid color based on how far it is from the average

            v1 = Rule1(boids[i]);
            v2 = Rule2(boids[i]);
            v3 = Rule3(boids[i]);

            boidVelocity[i] += v1 + v2 + v3;
            boidPositions[i] += boidVelocity[i];

            yield return new WaitForEndOfFrame();
        }
    }

    Vector3 Rule1(GameObject _boid)
    {
        return _boid.transform.position;
    }

    Vector3 Rule2(GameObject _boid)
    {
        return _boid.transform.position;
    }

    Vector3 Rule3(GameObject _boid)
    {
        return _boid.transform.position;
    }
}
