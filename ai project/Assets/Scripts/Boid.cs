using UnityEngine;

public class Boid
{
    public GameObject myObject;
    public Material myMat;
    public Vector3 position;
    public Vector3 velocity;
    public int quantity { get; set; }
    public float speed { get; set; }
    public float separationForce { get; set; }
    public float maxNeighbourDistance { get; set; }
    public float noise { get; set; }
    public float flock { get; set; }
    public bool followTarget { get; set; }
    public Gradient gradient { get; set; }

    private Vector3 currentVelocity = Vector3.zero;

    private BoidManager manager;

    public Boid(BoidManager _manager)
    {
        manager = _manager;
    }

    public void Update()
    {
        float val = Vector3.Distance(position, manager.averagePosition) / 20f;
        myMat.color = gradient.Evaluate(val);
        myMat.SetColor("_EmissionColor", gradient.Evaluate(val) * Mathf.Clamp(noise / 4, 0, 5));

        velocity += Force();
        velocity = velocity.normalized;
        currentVelocity = Vector3.MoveTowards(currentVelocity, velocity, (separationForce * Time.deltaTime));
        position += currentVelocity * Time.deltaTime * speed;
        myObject.transform.position = position;
    }

    // calculate boid direction and force
    Vector3 Force()
    {
        // follow target or stick near the center of the flock mass
        Vector3 boidDirection = Vector3.zero;
        if (followTarget)
        {
            boidDirection = manager.averagePosition;
            boidDirection -= position;
            boidDirection = boidDirection / flock;
        }
        else
        {
            boidDirection = manager.totalPosition - position;
            boidDirection *= (1f / (quantity - 1));
            boidDirection -= position;
            boidDirection = boidDirection / noise;
        }

        Vector3 boidVelocity = manager.totalVelocity - velocity;
        boidVelocity = boidVelocity / (quantity - 1);
        boidVelocity = (boidVelocity - velocity) / noise;

        Vector3 boidForce = Vector3.zero;
        Collider[] neighbours = Physics.OverlapSphere(position, maxNeighbourDistance);
        foreach (Collider neighbour in neighbours)
        {
            boidForce -= (manager.boidInstances[int.Parse(neighbour.name)].position - position);
        }

        return boidDirection + boidVelocity + boidForce;
    }
}
