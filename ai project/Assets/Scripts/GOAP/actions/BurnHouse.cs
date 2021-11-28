using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BurnHouse : Action, IAction
{
    void Awake()
    {
        actionName = "Burned House Down";
        name = actionName;
        hasRequirement = true;
        requiredItem = ItemList.ITEM_FIRE;
        actionCost = 4;
    }

    public void PerformAction()
    {

    }

    public void PerformAction(List<int> inventory)
    {

    }
}
