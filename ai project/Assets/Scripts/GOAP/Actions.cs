using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actions : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
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
        hasRequirement = false;
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