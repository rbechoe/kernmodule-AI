using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Action : MonoBehaviour
{
    public string actionName;
    public ItemList givenItem;
    public int givenAmount = 1;
    public int actionCost;
    public bool hasRequirement;
    public ItemList requiredItem;
    public int requiredAmount;

    // used for A-STAR
    public float FScore
    {
        get { return GScore + HScore; }
    }
    [HideInInspector]
    public int GScore;
    [HideInInspector]
    public int HScore;
    [HideInInspector]
    public Action parent;

    void Awake()
    {
        name = actionName;
    }

    public void PerformAction(EnemyAI EAI)
    {
        if (!hasRequirement)
        {
            Debug.Log("Action performed: " + name);
        }

        // TODO required amount implementation in the planning
        if (hasRequirement && EAI.HasRequirement(requiredItem, requiredAmount))
        {
            // TODO remove amount from inventory
            Debug.Log("Action performed: " + name);
        }
    }
}
