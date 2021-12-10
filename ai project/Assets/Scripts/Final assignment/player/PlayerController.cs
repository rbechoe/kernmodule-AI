using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamagable
{
    public float speed = 10;
    public float rotSpeed = 100;
    float attackedCd = 0;
    float attackedCdReset = 5;

    public bool attacked;
    public GameObject attacker;

    void Update()
    {
        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");
        transform.position += transform.forward * ver * Time.deltaTime * speed;
        transform.eulerAngles += new Vector3(0, hor, 0) * Time.deltaTime * rotSpeed;

        if (attacked)
        {
            attackedCd -= Time.deltaTime;
            if (attackedCd <= 0)
            {
                attacked = false;
            }
        }
    }

    public void TakeDamage(int amount, DamageType damageType, GameObject attacker)
    {
        this.attacker = attacker;
        attacked = true;
        attackedCd = attackedCdReset;
    }
}
