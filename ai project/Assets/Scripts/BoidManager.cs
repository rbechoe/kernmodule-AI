using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    [SerializeField]
    private GameObject boidPrefab;
    [SerializeField]
    private GameObject target;
    [SerializeField]
    private Vector3 minRange, maxRange;

    public List<Boid> boidInstances = new List<Boid>();
    private bool isDone;
    public Vector3 averagePosition;

    public int boidQuantity;
    public float boidSpeed = 100f; // higher = slower
    public float maxNeighbourDistance = 1f;
    public float boidSmooth = 8f;

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
            GameObject boidObj = Instantiate(boidPrefab, startPos, Quaternion.identity);
            boidObj.name = "" + i;

            boidInstances.Add(new Boid(i, this));
            boidInstances[i].myObject = boidObj;
            boidInstances[i].position = startPos;
            boidInstances[i].velocity = Vector3.zero;
            boidInstances[i].boidQuantity = boidQuantity;
            boidInstances[i].boidSpeed = boidSpeed;
            boidInstances[i].maxNeighbourDistance = maxNeighbourDistance;
            boidInstances[i].boidSmooth = boidSmooth;

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
        Vector3 posTotal = Vector3.zero;

        for (int i = 0; i < boidQuantity; i++)
        {
            boidInstances[i].Update();
            posTotal += boidInstances[i].position;
            //yield return new WaitForEndOfFrame();
        }

        averagePosition = posTotal / boidQuantity;
        target.transform.position = averagePosition;
        yield return new WaitForEndOfFrame();
        isDone = true;
    }
}
