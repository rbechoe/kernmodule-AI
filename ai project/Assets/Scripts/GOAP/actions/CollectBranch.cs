using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollectBranch : MonoBehaviour, IAction
{
    public string actionName;
    public int requiredItem;
    public int itemId;
    public int actionCost;
    public bool hasRequirement;

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
