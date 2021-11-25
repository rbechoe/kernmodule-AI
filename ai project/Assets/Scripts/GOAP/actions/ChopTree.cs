using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChopTree : MonoBehaviour, IAction
{
    public string actionName;
    public int requiredItem;
    public int itemId;
    public int actionCost;
    public bool hasRequirement;

    void Start()
    {
        name = "Chop Tree";
        requiredItem = ItemList.ITEM_AXE;
        itemId = ItemList.ITEM_BRANCH;
        actionCost = 10;
        hasRequirement = true;
    }

    public void PerformAction()
    {
        if (!hasRequirement) Debug.Log("Agent performed: " + name);
        // TODO receive a lot branches
    }

    public void PerformAction(List<int> inventory)
    {
        if (hasRequirement && inventory.Contains(requiredItem)) Debug.Log("Agent performed: " + name);
        else Debug.Log("Agent does not have item");
    }
}
