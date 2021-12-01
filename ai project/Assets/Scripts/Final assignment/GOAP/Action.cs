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

    // TODO implement perform action
    public void PerformAction(Inventory inventory)
    {
        if (!hasRequirement)
        {
            Debug.Log("Action performed: " + name);

            if (givenItem != ItemList.Empty)
            {
                inventory.AddToInventory(givenItem, givenAmount);
            }
        }

        if (hasRequirement && inventory.HasRequirement(requiredItem, requiredAmount))
        {
            Debug.Log("Action performed: " + name);

            if (givenItem != ItemList.Empty)
            {
                inventory.AddToInventory(givenItem, givenAmount);
            }

            if (hasRequirement)
            {
                inventory.RemoveFromInventory(requiredItem, requiredAmount);
            }
        }
    }
}
