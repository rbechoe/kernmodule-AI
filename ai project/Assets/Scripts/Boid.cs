using UnityEngine;

public class Boid
{
    public GameObject myObject;
    public Vector3 position;
    public Vector3 velocity;
    public int boidQuantity;
    public float boidSpeed;
    public float maxNeighbourDistance;
    public float boidSmooth;

    private int index;
    private BoidManager manager;

    public Boid(int _index, BoidManager _manager)
    {
        index = _index;
        manager = _manager;
    }

    public void Update()
    {
        Vector3 v1, v2, v3;

        // update each boid
        // move boid closer to average position
        // base speed on how far the boid is from the average
        // update boid color based on how far it is from the average

        v1 = Rule1();
        v2 = Rule2();
        v3 = Rule3();

        velocity += v1 + v2 + v3;
        velocity = Vector3.ClampMagnitude(velocity, boidSmooth);
        position += velocity;
        myObject.transform.position = position;
    }

    // calculate and move boid towards centre of mass
    Vector3 Rule1()
    {
        Vector3 boidDirection = Vector3.zero;

        for (int i = 0; i < boidQuantity; i++)
        {
            if (index != i)
            {
                boidDirection += manager.boidInstances[i].position;
            }
        }

        boidDirection = boidDirection * (1f / boidQuantity);
        boidDirection -= position;
        boidDirection = boidDirection / boidSpeed;

        return boidDirection;
    }

    // boid avoiding neighbour boid
    Vector3 Rule2()
    {
        Vector3 boidVelocity = Vector3.zero;

        Collider[] neighbours = Physics.OverlapSphere(position, maxNeighbourDistance);
        foreach (Collider neighbour in neighbours)
        {
            boidVelocity = boidVelocity - (position - manager.boidInstances[int.Parse(neighbour.name)].position);
        }

        return boidVelocity;
    }

    // average velocity compared to other boids
    Vector3 Rule3()
    {
        Vector3 boidVelocity = velocity;

        for (int i = 0; i < boidQuantity; i++)
        {
            if (index != i)
            {
                boidVelocity += manager.boidInstances[i].position;
            }
        }

        boidVelocity = boidVelocity / (boidQuantity - 1);
        boidVelocity = (boidVelocity - velocity) / boidSmooth;

        return boidVelocity;
    }
}
