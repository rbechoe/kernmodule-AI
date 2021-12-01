using UnityEngine;
using System.Collections;

public class AgentBehaviour : MonoBehaviour
{
    EnemyAI EAI;

    void Start()
    {
        if (gameObject.GetComponent<EnemyAI>() == null)
            EAI = gameObject.AddComponent<EnemyAI>();
        else
            EAI = gameObject.GetComponent<EnemyAI>();
    }

    void Update()
    {

    }
}
