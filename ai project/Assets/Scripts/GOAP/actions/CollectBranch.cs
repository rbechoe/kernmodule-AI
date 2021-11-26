using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollectBranch : Action, IAction
{
    void Start()
    {
        name = "Collect Branch";
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
