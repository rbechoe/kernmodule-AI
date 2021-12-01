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

    public void SelectRandomGoal(EnemyAI EAI)
    {
        if (actionsToGoal.Count == 0)
        {
            endGoal = availableActions[Random.Range(0, availableActions.Length)];
            CalculateOptimalRoute(EAI);
        }
    }

    public void SelectGoal(Action goal, EnemyAI EAI)
    {
        endGoal = goal;
        CalculateOptimalRoute(EAI);
    }

    void CalculateOptimalRoute(EnemyAI EAI)
    {
        pathToActions = FindPathToTarget(endGoal, EAI);
    }

    public List<Vector3> FindPathToTarget(Action goal, EnemyAI EAI)
    {
        List<Action> openSet = new List<Action>();
        HashSet<Action> closedSet = new HashSet<Action>();
        List<PickedAction> pickedActions = new List<PickedAction>();

        openSet.Add(goal);
        pickedActions.Add(new PickedAction(goal, null, 1));

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
            if (!currentAction.hasRequirement || EAI.inventory.HasRequirement(currentAction.requiredItem, currentAction.requiredAmount))
            {
                List<Vector3> path = new List<Vector3>();
                List<Action> nodePath = RetracePath(goal, EAI);
                for (int i = 0; i < nodePath.Count; i++)
                {
                    path.Add(nodePath[i].transform.position);
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
                // TODO: add each action individually to the list of actions instead of making a large timer
                // OPTIONAL: check if resource is depleted 
                // store heuristic based on amount of times action has to be done

                // sample data obj
                // Action action - action linked to it
                // Action parent - next action compared to this linked to it
                // int amount - quantity of how many times action has to be performed
                // int fscore - total score
                // int gscore - distance
                // int hscore - total action cost

                int distanceCost = Mathf.RoundToInt(Vector3.Distance(checkable.gameObject.transform.position, currentAction.transform.position));
                int reqCost = RequirementCost(checkable, EAI);
                int costToAction = currentAction.GScore + distanceCost + reqCost;
                if (costToAction < checkable.GScore || !openSet.Contains(checkable))
                {
                    checkable.GScore = distanceCost;
                    checkable.HScore = costToAction;
                    currentAction.parent = checkable;

                    if (!openSet.Contains(checkable))
                    {
                        openSet.Add(checkable);
                        // TODO pickedActions.Add(new PickedAction(goal, null, currentAction.requiredAmount / checkable.givenAmount));
                    }
                }
            }
        }

        Debug.Log("Couldnt find a path!");
        return null;
    }

    // Calculate costs based on actions and quantity
    public int RequirementCost(Action action, EnemyAI EAI)
    {
        int cost = 0;

        if (action.hasRequirement)
        {
            if (EAI.inventory.HasRequirement(action.requiredItem, 0))
            {
                cost = Mathf.CeilToInt((action.requiredAmount - EAI.inventory.HasAmountOfItem(action.requiredItem)) / action.givenAmount) * action.actionCost;
            }
            else
            {
                cost = Mathf.CeilToInt(action.requiredAmount / action.givenAmount) * action.actionCost;
            }
        }

        return cost;
    }

    // Retrace path and make sure that other lists get updated as well
    List<Action> RetracePath(Action goal, EnemyAI EAI)
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
                Debug.Log(currentAction.actionName + " takes " + currentAction.actionCost + " seconds");

                if (prevAction != currentAction)
                {
                    // TODO fix with amount already in inventory
                    for (int i = 0; i < (prevAction.requiredAmount - EAI.inventory.HasAmountOfItem(currentAction.givenItem)) / currentAction.givenAmount; i++)
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

public class PickedAction
{
    public Action action; // actual action
    public Action parent; // the action that required this action
    public int quantity; // amount of times this action has to be repeated

    public float FScore 
    {
        get { return GScore + HScore; }
    }
    public int GScore; // distance to action
    public int HScore; // total action cost

    public PickedAction() { }
    public PickedAction(Action action, Action parent, int quantity)
    {
        this.action = action;
        this.parent = parent;
        this.quantity = quantity;
    }
}
