using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPlanner : MonoBehaviour
{
    public Action[] availableActions;
    public Action endGoal;

    public List<Action> routeToGoal;
    public List<Vector3> pathToActions = new List<Vector3>();
    public List<int> inventory = new List<int>();

    private void Start()
    {
        availableActions = FindObjectsOfType<Action>();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            routeToGoal.Clear();
            SelectRandomGoal();
        }
    }

    public void SelectRandomGoal()
    {
        if (routeToGoal.Count == 0)
        {
            endGoal = availableActions[Random.Range(0, availableActions.Length)];
            CalculateOptimalRoute();
        }
    }

    public void SelectGoal(Action goal)
    {
        endGoal = goal;
        CalculateOptimalRoute();
    }

    void CalculateOptimalRoute()
    {
        pathToActions = FindPathToTarget(endGoal, availableActions);
    }

    public List<Vector3> FindPathToTarget(Action goal, Action[] actions)
    {
        List<Action> openSet = new List<Action>();
        HashSet<Action> closedSet = new HashSet<Action>();
        openSet.Add(goal);

        while (openSet.Count > 0)
        {
            Action currentAction = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (currentAction.hasRequirement && openSet[i].itemId == currentAction.requiredItem)
                {
                    currentAction = openSet[i];
                }
            }

            openSet.Remove(currentAction);
            closedSet.Add(currentAction);

            if (!currentAction.hasRequirement || inventory.Contains(currentAction.requiredItem))
            {
                List<Vector3> path = new List<Vector3>();
                List<Action> nodePath = RetracePath(goal);
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
                int reqCost = (checkable.hasRequirement && !inventory.Contains(checkable.requiredItem)) ? 10 : 0; // TODO define values properly
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

    List<Action> RetracePath(Action goal)
    {
        List<Action> path = new List<Action>();
        routeToGoal = new List<Action>();
        Action currentAction = goal;

        while (currentAction != null)
        {
            if (!path.Contains(currentAction))
            {
                path.Add(currentAction);
                routeToGoal.Add(currentAction);
                if (currentAction.parent != null)
                {
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

        routeToGoal.Reverse();
        path.Reverse();
        return path;
    }

    public List<Action> GetActions(Action action)
    {
        List<Action> options = new List<Action>();

        for (int i = 0; i < availableActions.Length; i++)
        {
            if (availableActions[i].itemId == action.requiredItem)
            {
                Action neighbour = availableActions[i];
                options.Add(neighbour);
            }
        }

        return options;
    }
}
