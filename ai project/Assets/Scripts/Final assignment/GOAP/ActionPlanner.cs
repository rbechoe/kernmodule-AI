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
        openSet.Add(goal);

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

                int distanceCost = Mathf.RoundToInt(Vector3.Distance(checkable.gameObject.transform.position, currentAction.transform.position));
                int reqCost = RequirementCost(checkable, EAI);
                int costToAction = currentAction.GScore + distanceCost + reqCost;
                if (costToAction < checkable.GScore || !openSet.Contains(checkable))
                {
                    checkable.GScore = costToAction;
                    checkable.HScore = distanceCost;
                    currentAction.parent = checkable;

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

    // Calculate costs based on actions
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
                // TODO: add each action individually to the list of actions instead of making a large timer
                int amountToGet = prevAction.requiredAmount - EAI.inventory.HasAmountOfItem(prevAction.requiredItem);
                float waitTime = Mathf.CeilToInt(amountToGet / currentAction.givenAmount) * currentAction.actionCost;
                Debug.Log(currentAction.actionName + " takes " + waitTime + " seconds");

                path.Add(currentAction);
                actionsToGoal.Add(currentAction);
                waitTimePerAction.Add(waitTime);

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
                Action neighbour = availableActions[i];
                options.Add(neighbour);
            }
        }

        return options;
    }
}
