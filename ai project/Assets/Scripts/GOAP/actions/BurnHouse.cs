using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BurnHouse : MonoBehaviour, IAction
{
    public string actionName;
    public int requiredItem;
    public int itemId;
    public int actionCost;
    public bool hasRequirement;

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
