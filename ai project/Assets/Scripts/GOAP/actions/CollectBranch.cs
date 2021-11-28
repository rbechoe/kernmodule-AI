using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollectBranch : Action, IAction
{
    void Awake()
    {
        actionName = "Collect Branch";
        name = actionName;
        itemId = ItemList.ITEM_BRANCH;
        actionCost = 2;
    }

    public void PerformAction()
    {

    }

    public void PerformAction(List<int> inventory)
    {

    }
}
