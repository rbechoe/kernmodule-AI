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
    public int FScore
    {
        get { return GScore + HScore; }
    }
    [HideInInspector]
    public int GScore;
    [HideInInspector]
    public int HScore;
    [HideInInspector]
    public Action parent;
    [HideInInspector]
    public Action child;
    [HideInInspector]
    public int quantity = 1;
    [HideInInspector]
    public int quantityStack; // stores total amount of quantity during this calculation in order to calculate actual quantity

    void Awake()
    {
        name = actionName;
    }
    
    public void PerformAction(Inventory inventory)
    {
        if (!hasRequirement)
        {
            Debug.Log("Action performed: " + name);

            if (givenItem != ItemList.Empty)
            {
                inventory.AddToInventory(givenItem, givenAmount);
            }

            return;
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

            return;
        }

        Debug.Log("Something went wrong when trying to complete action!");
    }
}
