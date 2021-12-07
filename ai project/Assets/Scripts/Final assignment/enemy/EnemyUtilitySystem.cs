using UnityEngine;
using System.Collections;

public class EnemyUtilitySystem
{
    EnemyAI EAI;

    float killIncrement = 1;
    float hungerIncrement = 0.1f;
    float tiredIncrement = 0.1f;
    float hpDecrement = 10000f;

    public float health { get; set; }
    public float aggroRange { get; set; }
    public float attackRange { get; set; }
    public float noDesireWeight { get; set; }
    public float desireToKill { get; set; }
    public float desireToEat { get; set; }
    public float desireToRest { get; set; }

    public EnemyUtilitySystem(EnemyAI parent)
    {
        EAI = parent;
        desireToKill = 0;
        desireToEat = 0;
        desireToRest = 0;
        noDesireWeight = 100;
        health = 100;
        aggroRange = 5;
        attackRange = 2;
    }

    // update enemy desires
    public void UpdateUtilities(float playerDistance)
    {
        desireToRest += (tiredIncrement + (1 - health / 100)) * Time.deltaTime;
        desireToEat += hungerIncrement * Time.deltaTime;
        health -= desireToEat / hpDecrement;

        if (playerDistance < aggroRange)
        {
            desireToKill += killIncrement;

            if (desireToKill > 80)
            {
                EAI.attackPlayer = true;
            }
        }
        else
        {
            desireToKill -= killIncrement / 10f * Time.deltaTime;
            EAI.attackPlayer = false;
        }

        desireToRest = Mathf.Clamp(desireToRest, 0, 100);
        desireToEat = Mathf.Clamp(desireToEat, 0, 100);
        desireToKill = Mathf.Clamp(desireToKill, 0, 100);
        health = Mathf.Clamp(health, 1, 100);
    }
}
