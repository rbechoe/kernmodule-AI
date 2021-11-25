using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollectAxe : MonoBehaviour, IAction
{
    public string actionName;
    public int requiredItem;
    public int itemId;
    public int actionCost;
    public bool hasRequirement;

    void Start()
    {
        name = "Collect Axe";
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
