using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actions : MonoBehaviour
{
    public List<Action> availableActions = new List<Action>();
}

public class BurnHouse : Action
{
    public override void PerformAction()
    {

    }

    public override void PerformAction(List<int> inventory)
    {

    }
}

public class CollectBranch : Action
{
    public override void PerformAction()
    {

    }

    public override void PerformAction(List<int> inventory)
    {

    }
}

public class StartFire : Action
{
    public override void PerformAction()
    {

    }

    public override void PerformAction(List<int> inventory)
    {

    }
}

public class ChopTree : Action
{
    ChopTree()
    {
        name = "Chop Tree";
        requiredItem = ItemList.ITEM_AXE;
        hasRequirement = true;
    }

    public override void PerformAction()
    {
        if (!hasRequirement) Debug.Log("Agent performed: " + name);
    }

    public override void PerformAction(List<int> inventory)
    {
        if (hasRequirement && inventory.Contains(requiredItem)) Debug.Log("Agent performed: " + name);
        else Debug.Log("Agent does not have item");
    }
}

public class CollectAxe : Action
{
    CollectAxe()
    {
        name = "Collect Axe";
        itemId = ItemList.ITEM_AXE;
        hasRequirement = false;
    }

    public override void PerformAction()
    {
        if (!hasRequirement) Debug.Log("Agent performed: " + name);
    }

    public override void PerformAction(List<int> inventory)
    {
        // TODO give item to inventory
        Debug.Log("Agent performed: " + name);
    }
}