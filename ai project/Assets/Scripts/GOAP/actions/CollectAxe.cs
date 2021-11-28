using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollectAxe : Action, IAction
{
    void Awake()
    {
        actionName = "Collect Axe";
        name = actionName;
        itemId = ItemList.ITEM_AXE;
        actionCost = 3;
        hasRequirement = false;
    }

    public void PerformAction()
    {
        if (!hasRequirement) Debug.Log("Agent performed: " + name);
    }

    public void PerformAction(List<int> inventory)
    {
        // TODO give item to inventory
        Debug.Log("Agent performed: " + name);
    }
}
