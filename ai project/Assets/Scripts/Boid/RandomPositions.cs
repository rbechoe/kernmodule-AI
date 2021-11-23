using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPositions : MonoBehaviour
{
    [Range(1, 60)]
    public float maxCooldown = 10;
    public Vector3 range;

    float cooldown;

    void Update()
    {
        cooldown -= Time.deltaTime;
        if (cooldown <= 0)
        {
            cooldown = maxCooldown;
            transform.position = new Vector3(Random.Range(-range.x, range.x),
                                             Random.Range(-range.y, range.y),
                                             Random.Range(-range.z, range.z));
        }
    }
}
