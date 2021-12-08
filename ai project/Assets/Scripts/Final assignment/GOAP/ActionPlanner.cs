using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPlanner : MonoBehaviour
{
    public Action[] availableActions;
    public Action endGoal;

    public List<float> waitTimePerAction;
    public List<Action> actionsToGoal;
    public List<Vector3> pathToActions = new List<Vector3>();

    private void Start()
    {
        availableActions = FindObjectsOfType<Action>();
    }

    public void SelectRandomGoal(Inventory inventory)
    {
        if (actionsToGoal.Count == 0)
        {
            endGoal = availableActions[Random.Range(0, availableActions.Length)];
            CalculateOptimalRoute(inventory);
        }
    }

    public void SelectGoal(Action goal, Inventory inventory)
    {
        endGoal = goal;
        CalculateOptimalRoute(inventory);
    }

    public void CollectSpecificItem(ItemList item, int amount, Inventory inventory, Vector3 position)
    {
        endGoal = null;
        float estimatedCost = float.MaxValue;

        foreach(Action action in availableActions)
        {
            if (action.givenItem != item) continue;
            if (amount * action.actionCost + Vector3.Distance(action.transform.position, position) < estimatedCost)
            {
                estimatedCost = amount * action.actionCost + Vector3.Distance(action.transform.position, position);
                endGoal = action;
            }
        }

        pathToActions = FindPathToTarget(endGoal, inventory, amount);
    }

    void CalculateOptimalRoute(Inventory inventory)
    {
        pathToActions = FindPathToTarget(endGoal, inventory);
    }

    public List<Vector3> FindPathToTarget(Action goal, Inventory inventory, int amount = 1)
    {
        List<Action> openSet = new List<Action>();
        HashSet<Action> closedSet = new HashSet<Action>();
        openSet.Add(goal);
        goal.quantityStack = goal.requiredAmount;

        while (openSet.Count > 0)
        {
            Action currentAction = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (currentAction.hasRequirement && openSet[i].givenItem == currentAction.requiredItem)
                {
                    currentAction = openSet[i];
                }
            }

            openSet.Remove(currentAction);
            closedSet.Add(currentAction);

            // if the action has no requirement or if the enemy has a sufficient amount of the item return a route
            if (!currentAction.hasRequirement || inventory.HasRequirement(currentAction.requiredItem, currentAction.requiredAmount))
            {
                List<Vector3> path = new List<Vector3>();
                List<Action> actionPath = RetracePath(goal, inventory, amount);
                for (int i = 0; i < actionPath.Count; i++)
                {
                    path.Add(actionPath[i].transform.position);
                }
                return path;
            }
            
            foreach (Action checkable in GetActions(currentAction))
            {
                // skip this iteration if action is unreachable or if we already checked it
                if (closedSet.Contains(checkable))
                {
                    continue;
                }

                // TODO: once full path is calculated try path that had branch to see if it is more efficient
                // store each path in a dictionary which also has the total action cost and then pick the lowest costing one
                // after reaching the first path give a hard cap to calculation time in order to prevent AI from thinking for too long

                // TODO: add each action individually to the list of actions instead of making a large timer

                // OPTIONAL: check if resource is depleted 

                int distanceCost = Mathf.RoundToInt(Vector3.Distance(checkable.gameObject.transform.position, currentAction.transform.position));
                int reqCost = RequirementCost(currentAction, checkable, inventory);
                int costToAction = currentAction.FScore + reqCost;
                if (costToAction < checkable.FScore || !openSet.Contains(checkable))
                {
                    currentAction.parent = checkable;
                    checkable.child = currentAction;
                    checkable.quantityStack = currentAction.quantityStack * checkable.requiredAmount;
                    checkable.GScore = distanceCost;
                    checkable.HScore = costToAction;
                    checkable.quantity = CalculateAmountNeeded(currentAction, checkable, inventory);

                    if (!openSet.Contains(checkable))
                    {
                        openSet.Add(checkable);
                    }
                }
            }
        }

        Debug.Log("Couldnt find a path!");
        return null;
    }

    // Calculate costs based on actions and quantity
    public int RequirementCost(Action requirement, Action giver, Inventory inventory)
    {
        int cost = 0;

        if (requirement.hasRequirement)
        {
            cost = CalculateAmountNeeded(requirement, giver, inventory) * giver.actionCost;
        }

        return cost;
    }

    // Calculate how many items are needed
    public int CalculateAmountNeeded(Action requirement, Action giver, Inventory inventory)
    {
        int amount = 0;
        amount = Mathf.CeilToInt((requirement.requiredAmount * requirement.quantityStack - inventory.HasAmountOfItem(requirement.requiredItem)) / giver.givenAmount);
        return amount;
    }

    // Retrace path and make sure that other lists get updated as well
    List<Action> RetracePath(Action goal, Inventory inventory, int _amount)
    {
        List<Action> path = new List<Action>();
        Action currentAction = goal;
        Action prevAction = goal;
        actionsToGoal = new List<Action>();
        waitTimePerAction = new List<float>();

        while (currentAction != null)
        {
            if (!path.Contains(currentAction))
            {
                int amount = (currentAction.child != null) ? currentAction.child.quantityStack : _amount;

                if (prevAction != currentAction)
                {
                    for (int i = 0; i < (amount - inventory.HasAmountOfItem(currentAction.givenItem)) / currentAction.givenAmount; i++)
                    {
                        path.Add(currentAction);
                        actionsToGoal.Add(currentAction);
                        waitTimePerAction.Add(currentAction.actionCost);
                    }
                }
                else
                {
                    path.Add(currentAction);
                    actionsToGoal.Add(currentAction);
                    waitTimePerAction.Add(currentAction.actionCost);
                }

                if (currentAction.parent != null)
                {
                    prevAction = currentAction;
                    currentAction = currentAction.parent;
                }
                else
                {
                    break;
                }
            }
            else
            {
                Debug.Log("Failed to calculate whole path, triggered infinite loop");
                break;
            }
        }

        actionsToGoal.Reverse();
        waitTimePerAction.Reverse();
        path.Reverse();
        return path;
    }

    public List<Action> GetActions(Action action)
    {
        List<Action> options = new List<Action>();

        for (int i = 0; i < availableActions.Length; i++)
        {
            if (availableActions[i].givenItem == action.requiredItem)
            {
                Action option = availableActions[i];
                options.Add(option);
            }
        }

        return options;
    }
}
