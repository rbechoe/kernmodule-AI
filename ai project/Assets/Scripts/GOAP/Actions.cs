using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actions : MonoBehaviour
{
    public Action[] availableActions;
    // TODO make scriptable objects for actions to make it super generic

    // calculate distances between nodes

    // calculate individual node cost

    // use A* to calculate routes

    public Action endGoal;

    public List<Action> routeToGoal;

    public List<Vector3> pathToActions = new List<Vector3>();

    public List<int> inventory = new List<int>();

    private void Start()
    {
        availableActions = FindObjectsOfType<Action>();

        SelectNewGoal();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            SelectNewGoal();
        }
    }

    void SelectNewGoal()
    {
        if (routeToGoal.Length == 0)
        {
            endGoal = availableActions[Random.Range(0, availableActions.Length)];
            CalculateOptimalRoute();
        }
    }

    void CalculateOptimalRoute()
    {
        // TODO make temporal inventory in order to ensure if character will meet requirements when reaching node
        // TODO check per node in temp inventory
        pathToActions = FindPathToTarget(transform.position, endGoal, availableActions);
    }

    public List<Vector3> FindPathToTarget(Vector3 startPosition, Action goal, Action[] actions)
    {
        List<Action> openSet = new List<Action>();
        List<int> tempInventory = inventory;
        HashSet<Action> closedSet = new HashSet<Action>();
        openSet.Add(goal);
        // Logic: start at the goal and check for requirements to find eventually the most optimal path

        while (openSet.Count > 0)
        {
            Action currentAction = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (!openSet[i].hasRequirement || (openSet[i].hasRequirement && inventory.Contains(openSet[i].requiredItem)))
                {
                    currentAction = openSet[i];
                }
            }

            openSet.Remove(currentAction);
            closedSet.Add(currentAction);

            if (!currentAction.hasRequirement)
            {
                List<Vector3> path = new List<Vector3>();
                List<Action> nodePath = RetracePath(goal);
                for (int i = 0; i < nodePath.Count; i++)
                {
                    path.Add(nodePath[i].transform.position);
                }
                return path;
            }

            // TODO check each action that meets requirement
            foreach (Action checkable in GetActions(currentAction))
            {
                // skip this iteration if action is unreachable or if we already checked it
                if (closedSet.Contains(checkable) || checkable.hasRequirement && !tempInventory.Contains(checkable.requiredItem))
                {
                    continue;
                }

                int distanceCost = Mathf.RoundToInt(Vector3.Distance(checkable.gameObject.transform.position, currentAction.transform.position));
                int costToAction = currentAction.GScore + distanceCost;
                if (costToAction < checkable.GScore || !openSet.Contains(checkable))
                {
                    checkable.GScore = costToAction;
                    checkable.HScore = distanceCost;
                    checkable.parent = currentAction;

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
                if (!currentAction.hasRequirement)
                {
                    break;
                }
                path.Add(currentAction);
                currentAction = currentAction.parent;
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
