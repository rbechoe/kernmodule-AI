using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public int boidQuantity;

    public GameObject boidPrefab;
    public GameObject target;

    public List<GameObject> boids;

    public List<Vector3> boidPositions;
    public List<Vector3> boidVelocities;
    public Vector3 averagePosition;

    public Vector3 minRange, maxRange;

    private float boidSpeed = 100f; // higher = slower
    private float maxNeighbourDistance = 1f;
    private float boidSmooth = 8f;
    private bool isDone;

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
            boidVelocities.Add(Vector3.zero);
            posTotal += startPos;
            yield return new WaitForEndOfFrame();
        }
        averagePosition = posTotal / boidQuantity;
        target.transform.position = averagePosition;
        isDone = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDone) StartCoroutine(UpdateBoids());
    }

    IEnumerator UpdateBoids()
    {
        isDone = false;
        Vector3 v1 = Vector3.zero;
        Vector3 v2 = Vector3.zero;
        Vector3 v3 = Vector3.zero;
        Vector3 posTotal = Vector3.zero;

        for (int i = 0; i < boidQuantity; i++)
        {
            // update each boid
            // move boid closer to average position
            // base speed on how far the boid is from the average
            // update boid color based on how far it is from the average

            v1 = Rule1(i);
            v2 = Rule2(i);
            v3 = Rule3(i);

            boidVelocities[i] += v1 + v2 + v3;
            boidVelocities[i] = Vector3.ClampMagnitude(boidVelocities[i], 8);
            boidPositions[i] += boidVelocities[i];

            boids[i].transform.position = boidPositions[i];
            posTotal += boidPositions[i];
            yield return new WaitForEndOfFrame();
        }

        averagePosition = posTotal / boidQuantity;
        target.transform.position = averagePosition;
        isDone = true;
    }

    // calculate and move boid towards centre of mass
    Vector3 Rule1(int _boid)
    {
        Vector3 boidDirection = Vector3.zero;

        for (int i = 0; i < boids.Count; i++)
        {
            if (_boid != i)
            {
                boidDirection += boidPositions[i];
            }
        }


        float average = 1f / (boids.Count);
        boidDirection = boidDirection * average;
        boidDirection -= boidPositions[_boid];
        boidDirection = boidDirection / boidSpeed;

        return boidDirection;
    }

    // boid avoiding neighbour boid
    Vector3 Rule2(int _boid)
    {
        Vector3 boidVelocity = Vector3.zero;

        for (int i = 0; i < boids.Count; i++)
        {
            if (_boid != i)
            {
                if (Vector3.Distance(boidPositions[_boid], boidPositions[i]) < maxNeighbourDistance)
                {
                    boidVelocity = boidVelocity - (boidPositions[_boid] - boidPositions[i]);
                }
            }
        }

        return boidVelocity;
    }

    // average velocity compared to other boids
    Vector3 Rule3(int _boid)
    {
        Vector3 boidVelocity = boidVelocities[_boid];

        for (int i = 0; i < boids.Count; i++)
        {
            if (_boid != i)
            {
                boidVelocity += boidVelocities[i];
            }
        }

        boidVelocity = boidVelocity / (boids.Count - 1);
        boidVelocity = (boidVelocity - boidVelocities[_boid]) / boidSmooth;

        return boidVelocity;
    }
}
