using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject explosionEffect;
    float aliveTimer = 10;

    void Update()
    {
        aliveTimer -= Time.deltaTime;

        if (aliveTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
        collision.gameObject.GetComponent<IDamagable>()?.TakeDamage(1, DamageType.Daze, gameObject);
        Destroy(gameObject);
    }
}
