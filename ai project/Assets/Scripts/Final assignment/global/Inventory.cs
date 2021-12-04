using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory
{
    Dictionary<ItemList, int> items = new Dictionary<ItemList, int>(); // item type, amount

    // return if inventory contains
    public bool HasItem(ItemList item)
    {
        return items.ContainsKey(item);
    }

    // return amount of item in inventory
    public int HasAmountOfItem(ItemList item)
    {
        if (items.ContainsKey(item))
        {
            return items[item];
        }
        else
        {
            return 0;
        }
    }

    // check if requirement has been met
    public bool HasRequirement(ItemList requiredItem, int requiredAmount)
    {
        return (items.ContainsKey(requiredItem) && items[requiredItem] > requiredAmount);
    }

    // add x items to inventory
    public void AddToInventory(ItemList item, int amount)
    {
        Debug.Log("Added " + item + " " + amount + " times");
        if (items.ContainsKey(item))
        {
            items[item] += amount;
        }
        else
        {
            items.Add(item, amount);
        }
    }

    // remove x items from inventory
    public void RemoveFromInventory(ItemList item, int amount)
    {
        Debug.Log("Removed " + item + " " + amount + " times");
        items[item] -= amount;
        if (items[item] < 0) Debug.Log("Somehow managed to get " + item + " with amount " + items[item]);
    }

    // return all keys
    public ItemList[] GetKeys()
    {
        return items.Keys.ToArray();
    }

    // return all values
    public int[] GetValues()
    {
        return items.Values.ToArray();
    }
}
