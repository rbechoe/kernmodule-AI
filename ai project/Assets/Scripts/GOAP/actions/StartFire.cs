using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StartFire : Action, IAction
{
    void Awake()
    {
        actionName = "Start Fire";
        name = actionName;
        itemId = ItemList.ITEM_FIRE;
        actionCost = 5;
        hasRequirement = true;
        requiredItem = ItemList.ITEM_BRANCH;
        // TODO define required amount
    }

    public void PerformAction()
    {

    }

    public void PerformAction(List<int> inventory)
    {

    }
}
