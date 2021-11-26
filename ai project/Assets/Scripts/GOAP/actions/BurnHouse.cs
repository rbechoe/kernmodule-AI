using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BurnHouse : Action, IAction
{
    void Start()
    {
        actionName = "Burned House Down";
        itemId = ItemList.ITEM_FIRE;
        actionCost = 4;
    }

    public void PerformAction()
    {

    }

    public void PerformAction(List<int> inventory)
    {

    }
}
