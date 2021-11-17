using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    private bool isDone;
    public float movementSpeed = 5;

    //---------------used for maths-----------------------------
    [HideInInspector]
    public List<Boid> boidInstances = new List<Boid>();
    [HideInInspector]
    public Vector3 averagePosition;
    [HideInInspector]
    public Vector3 totalPosition;
    [HideInInspector]
    public Vector3 totalVelocity;

    //---------------things to set------------------------------
    [Header("Assign the following")]
    [SerializeField]
    private GameObject boidPrefab;
    [SerializeField]
    private GameObject centerMass;
    [SerializeField]
    private GameObject targetObject;

    //---------------settings for the game designer-------------
    [Header("Adjust the following")]
    [Tooltip("Set the range for the spawn of the boids")]
    public Vector3 range;
    [Tooltip("Set amount of boids")]
    [Range(1, 10000)]
    public int boidQuantity;
    [Tooltip("Update boid seperation")] 
    [Range(1, 10)]
    public float maxNeighbourDistance = 1f;
    [Tooltip("Update boid speed")]
    [Range(1, 50)]
    public float boidSpeed = 5f;
    [Tooltip("Update boid step, used for velocity")]
    [Range(1, 500)]
    public float boidStep = 100f;
    [Tooltip("Update boid smooth, used for velocity")]
    [Range(1, 500)]
    public float boidSmooth = 100f;

    void Start()
    {
        Vector3 posTotal = Vector3.zero;
        for (int i = 0; i < boidQuantity; i++)
        {
            Vector3 startPos = new Vector3(Random.Range(-range.x, range.x),
                                           Random.Range(-range.y, range.y),
                                           Random.Range(-range.z, range.z));
            GameObject boidObj = Instantiate(boidPrefab, startPos, Quaternion.identity);
            boidObj.name = "" + i;

            boidInstances.Add(new Boid(this));
            boidInstances[i].myObject = boidObj;
            boidInstances[i].myMat = boidObj.GetComponent<Renderer>().material;
            boidInstances[i].position = startPos;
            boidInstances[i].velocity = Vector3.zero;
            boidInstances[i].quantity = boidQuantity;
            boidInstances[i].UpdateSettings(boidSpeed, maxNeighbourDistance, boidSmooth, boidStep);

            posTotal += startPos;
        }
        isDone = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDone) StartCoroutine(UpdateBoids());

        averagePosition = totalPosition / boidQuantity;
        // TODO make boids follow the averagePosition in order to move as a hive
        averagePosition += (targetObject.transform.position - averagePosition).normalized * Time.deltaTime * movementSpeed;
        centerMass.transform.position = averagePosition;
    }

    IEnumerator UpdateBoids()
    {
        isDone = false;
        totalPosition = Vector3.zero;
        totalVelocity = Vector3.zero;

        // get total values
        for (int i = 0; i < boidQuantity; i++)
        {
            totalPosition += boidInstances[i].position;
            totalVelocity += boidInstances[i].velocity;
            boidInstances[i].UpdateSettings(boidSpeed, maxNeighbourDistance, boidSmooth, boidStep);
        }

        // update individual boids
        for (int i = 0; i < boidQuantity; i++)
        {
            boidInstances[i].Update();
        }

        isDone = true;
        yield return new WaitForEndOfFrame();
    }
}
