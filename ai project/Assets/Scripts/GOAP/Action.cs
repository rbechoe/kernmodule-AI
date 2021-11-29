using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Action : MonoBehaviour
{
    public string actionName;
    public int itemId;
    public int actionCost;
    public bool hasRequirement;
    public int requiredItem;
    public int requiredAmount;

    // used for A-STAR
    public float FScore
    {
        get { return GScore + HScore; }
    }
    [Header("Used for A-Star, do not change!")]
    public int GScore;
    public int HScore;
    public Action parent;

    void Awake()
    {
        name = actionName;
    }

    public void PerformAction()
    {
        if (!hasRequirement)
        {
            Debug.Log("Action performed: " + name);
        }
    }

    public void PerformAction(List<int> inventory)
    {
        if (hasRequirement && inventory.Contains(requiredItem))
        {
            // TODO remove amount
            Debug.Log("Action performed: " + name);
        }
    }
}
