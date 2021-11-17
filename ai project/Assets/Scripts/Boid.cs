using UnityEngine;

public class Boid
{
    public GameObject myObject;
    public Material myMat;
    public Vector3 position;
    public Vector3 velocity;
    public int quantity;
    public float speed;
    public float maxNeighbourDistance;
    public float smooth;
    public float step;

    private BoidManager manager;

    public Boid(BoidManager _manager)
    {
        manager = _manager;
    }

    public void Update()
    {
        float val = Vector3.Distance(position, manager.averagePosition) / 20f;
        myMat.color = new Color(1 - val, val, 0);

        Vector3 v1, v2, v3;
        v1 = Rule1();
        v2 = Rule2();
        v3 = Rule3();

        velocity += v1 + v2 + v3;
        velocity = velocity.normalized;
        position += velocity * Time.deltaTime * speed;
        myObject.transform.position = position;
    }

    // Update settings
    public void UpdateSettings(float _speed, float _maxNeighbourDistance, float _smooth, float _step)
    {
        speed = _speed;
        maxNeighbourDistance = _maxNeighbourDistance;
        smooth = _smooth;
        step = _step;
    }

    // calculate and move boid towards centre of mass
    Vector3 Rule1()
    {
        Vector3 boidDirection = manager.totalPosition - position;

        boidDirection = boidDirection * (1f / (quantity - 1));
        boidDirection -= position;
        boidDirection = boidDirection / step;

        return boidDirection;
    }

    // boid avoiding neighbour boid
    Vector3 Rule2()
    {
        Vector3 boidVelocity = Vector3.zero;

        Collider[] neighbours = Physics.OverlapSphere(position, maxNeighbourDistance);
        foreach (Collider neighbour in neighbours)
        {
            boidVelocity -= (manager.boidInstances[int.Parse(neighbour.name)].position - position);
        }

        return boidVelocity;
    }

    // average velocity compared to other boids
    Vector3 Rule3()
    {
        Vector3 boidVelocity = manager.totalVelocity - velocity;

        boidVelocity = boidVelocity / (quantity - 1);
        boidVelocity = (boidVelocity - velocity) / smooth;

        return boidVelocity;
    }
}
