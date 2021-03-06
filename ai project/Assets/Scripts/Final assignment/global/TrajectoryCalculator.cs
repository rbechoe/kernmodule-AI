using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryCalculator : MonoBehaviour
{
    // DISCLAIMER
    // This was based on a tutorial, I already had made similar systems in the past, but didn't have the time to come up with something
    // so I used a tutorial for this in order to get it done quick
    // all credits to Romi Fauzi for the logic
    // https://www.youtube.com/watch?v=03GHtGyEHas

    public Rigidbody bombPrefab;
    public GameObject spawnPoint;

    public void LaunchProjectile(GameObject target)
    {
        Vector3 Vo = CalculateVelocity(target.transform.position, transform.position, 1f);
        transform.rotation = Quaternion.LookRotation(Vo);

        Rigidbody obj = Instantiate(bombPrefab, spawnPoint.transform.position, Quaternion.identity);
        obj.velocity = Vo;
    }

    Vector3 CalculateVelocity(Vector3 target, Vector3 origin, float time)
    {
        Vector3 distance = target - origin;
        Vector3 distanceXZ = distance;
        distance.Normalize();
        distanceXZ.y = target.y;

        float Sy = distance.y;
        float Sxz = distanceXZ.magnitude;

        float Vxz = Sxz / time;
        float Vy = Sy / time + 0.5f * Mathf.Abs(Physics.gravity.y) * time;

        Vector3 result = distanceXZ.normalized;
        result *= Vxz;
        result.y = Vy;

        return result;
    }
}
