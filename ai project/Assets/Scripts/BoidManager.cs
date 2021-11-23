using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.WasapiAudio.Scripts.Unity;

public class BoidManager : AudioVisualizationEffect
{
    private bool isDone;

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
    [Header("Assign before running")]
    [SerializeField]
    private GameObject boidPrefab;
    [SerializeField]
    private GameObject centerMass;
    [SerializeField]
    private GameObject targetObject;

    //---------------settings for the game designer-------------
    [Header("Settings")]
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
    [Tooltip("Update boid flocking behaviour")]
    [Range(1, 500)]
    public float boidFlocking = 100f;
    [Tooltip("Update boid uniqueness")]
    [Range(1, 500)]
    public float boidNoise = 5f;
    [Tooltip("Update how aggressively boids avoid each other")]
    [Range(1, 100)]
    public float boidSeparationForce = 5f;
    public Gradient boidGradient;

    //---------------extra settings for the game designer------
    [Header("Extra settings")]
    [Tooltip("How fast do we move to the target?")]
    [Range(1, 100)]
    public float movementSpeed = 5;
    [Tooltip("Follow target or stick near center of flock mass")]
    public bool followTarget;
    [Tooltip("Yup!")]
    public bool enableMusic;

    //---------------audio related multipliers-----------------
    [Header("Audio related multipliers")]
    [Tooltip("Neighbour distance multiplier")]
    [Range(1, 100)]
    public float NeighbourDistMult = 10f;
    [Tooltip("Speed multiplier")]
    [Range(1, 100)]
    public float speedMult = 40f;
    [Tooltip("Flocking multiplier")]
    [Range(1, 100)]
    public float flockMult = 20f;
    [Tooltip("Noise multiplier")]
    [Range(1, 100)]
    public float noiseMult = 20f;
    [Tooltip("Separation force multiplier")]
    [Range(1, 100)]
    public float sepaMult = 20f;

    //---------------profile presets---------------------------
    [Header("Audio related multipliers")]
    [Tooltip("Apply first profile to the multipliers")]
    public bool applyProfile1;
    [Tooltip("Apply second profile to the multipliers")]
    public bool applyProfile2;
    [Tooltip("Apply third profile to the multipliers")]
    public bool applyProfile3;

    //---------------audio related stuff-----------------------
    [Header("Audio settings")]
    private int sampleSize = 128;
    private float[] samples;
    private float rmsValue;
    [Range(1,20)]
    public float rmsMultiplier = 3f;

    void Start()
    {
        sampleSize = WasapiAudioSource.SpectrumSize;
        samples = new float[sampleSize];

        Vector3 posTotal = Vector3.zero;
        for (int i = 0; i < boidQuantity; i++)
        {
            Vector3 startPos = new Vector3(Random.Range(-range.x, range.x),
                                           Random.Range(-range.y, range.y),
                                           Random.Range(-range.z, range.z));
            GameObject boidObj = Instantiate(boidPrefab, startPos, Quaternion.identity);
            boidObj.name = "" + i;
            boidObj.tag = "boid";

            boidInstances.Add(new Boid(this));
            boidInstances[i].myObject = boidObj;
            boidInstances[i].myMat = boidObj.GetComponent<Renderer>().material;
            boidInstances[i].myMat.EnableKeyword("_EMISSION");
            boidInstances[i].position = startPos;
            boidInstances[i].velocity = Vector3.zero;
            boidInstances[i].quantity = boidQuantity;
            UpdateBoidSettings(boidInstances[i]);

            posTotal += startPos;
        }
        isDone = true;
    }

    void Update()
    {
        if (isDone) StartCoroutine(UpdateBoids());
        if (applyProfile1 || applyProfile2 || applyProfile3) SetProfile();

        // go to target position or calculate center of the flock mass
        if (followTarget)
        {
            averagePosition += (targetObject.transform.position - averagePosition).normalized * Time.deltaTime * movementSpeed;
        }
        else
        {
            averagePosition = totalPosition / boidQuantity;
            averagePosition += (targetObject.transform.position - averagePosition).normalized * Time.deltaTime * movementSpeed;
        }

        // update the audio based behaviour
        if (enableMusic)
        {
            AnalyzeSound();
            maxNeighbourDistance = 1 + rmsValue * NeighbourDistMult;
            boidSpeed = 5 + rmsValue * speedMult;
            boidFlocking = (rmsValue + 10) * flockMult;
            boidNoise = 10 + rmsValue * noiseMult;
            boidSeparationForce = 5 + rmsValue * sepaMult;
        }
        
        centerMass.transform.position = averagePosition;
    }

    private void SetProfile()
    {
        if (applyProfile1)
        {
            NeighbourDistMult = 1;
            speedMult = 25;
            flockMult = 5;
            noiseMult = 20;
            sepaMult = 7;
            rmsMultiplier = 6.9f;
            applyProfile1 = false;
        }

        if (applyProfile2)
        {
            NeighbourDistMult = 1;
            speedMult = 30;
            flockMult = 5;
            noiseMult = 20;
            sepaMult = 5;
            rmsMultiplier = 5;
            applyProfile2 = false;
        }

        if (applyProfile3)
        {
            NeighbourDistMult = 1;
            speedMult = 10;
            flockMult = 1;
            noiseMult = 25;
            sepaMult = 1;
            rmsMultiplier = 10;
            applyProfile3 = false;
        }
    }

    private void AnalyzeSound()
    {
        samples = GetSpectrumData();

        // Get RMS
        float sum = 0;
        for (int i = 0; i < sampleSize; i++)
        {
            sum += samples[i] * samples[i];
        }
        rmsValue = Mathf.Sqrt(sum / sampleSize) * rmsMultiplier;
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
            UpdateBoidSettings(boidInstances[i]);
        }

        // update individual boids
        for (int i = 0; i < boidQuantity; i++)
        {
            boidInstances[i].Update();
        }

        isDone = true;
        yield return new WaitForEndOfFrame();
    }

    void UpdateBoidSettings(Boid _boid)
    {
        _boid.speed = boidSpeed;
        _boid.maxNeighbourDistance = maxNeighbourDistance;
        _boid.noise = boidNoise;
        _boid.flock = boidFlocking;
        _boid.separationForce = boidSeparationForce;
        _boid.followTarget = followTarget;
        _boid.gradient = boidGradient;
    }
}
