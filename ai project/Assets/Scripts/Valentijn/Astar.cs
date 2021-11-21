using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Astar
{
    /// <summary>
    /// TODO: Implement this function so that it returns a list of Vector2Int positions which describes a path
    /// Note that you will probably need to add some helper functions
    /// from the startPos to the endPos
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="grid"></param>
    /// <returns></returns>
    public List<Vector2Int> FindPathToTarget(Vector2Int startPos, Vector2Int endPos, Cell[,] grid)
    {
        Debug.Log("calculating path...");
        Node startNode = new Node(startPos, null, grid[startPos.x, startPos.y].index);
        Node endNode = new Node(endPos, null, grid[endPos.x, endPos.y].index);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        // TODO make dictionary that stores index as key and node as value
        openSet.Add(startNode); // starting point for the pathfinding

        int count = 10;
        while (openSet.Count > 0)
        {
            count--;
            if (count <= 0)
            {
                Debug.Log("something went wrong...");
                List<Vector2Int> path = new List<Vector2Int>();
                List<Node> nodePath = RetracePath(startNode, endNode);
                for (int i = 0; i < nodePath.Count; i++)
                {
                    path.Add(nodePath[i].position);
                }
                return path;
            }

            //Debug.Log(openSet.Count + " - " + closedSet.Count); // <-- loops infinitely
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                // if other node(s) have better scores start looking from their point
                if (openSet[i].FScore < currentNode.FScore || openSet[i].FScore == currentNode.FScore && openSet[i].HScore < currentNode.HScore)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == endNode)
            {
                Debug.Log("Succesfully found a path!");
                List<Vector2Int> path = new List<Vector2Int>();
                List<Node> nodePath = RetracePath(startNode, endNode);
                for (int i = 0; i < nodePath.Count; i++)
                {
                    path.Add(nodePath[i].position);
                }
                return path;
            }

            foreach (Node neighbour in GetNeighbours(currentNode, grid))
            {
                // TODO implement wallcheck between this node and previous node
                if (closedSet.Contains(neighbour))
                {
                    continue;
                }

                int costToNeighbour = currentNode.GScore + GetDistance(currentNode, neighbour);
                if (costToNeighbour < neighbour.GScore || !openSet.Contains(neighbour))
                {
                    neighbour.GScore = costToNeighbour;
                    neighbour.HScore = GetDistance(neighbour, endNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        // TODO: somehow always makes it to here past the checks
                        // TODO: check by index if it already exists
                        //Debug.Log("added neighbour " + neighbour.position);
                        openSet.Add(neighbour);
                    }
                }
            }
        }

        Debug.Log("Couldnt find a path!");
        return null;
    }

    // Create reversed path for the agent to navigate
    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        return path;
    }

    // Calculate distance between A and B
    int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
        int distY = Mathf.Abs(nodeA.position.y - nodeB.position.y);

        if (distX > distY)
        {
            return 14 * distY + 10 * (distX - distY);
        }
        else
        {
            return 14 * distX + 10 * (distY - distX);
        }
    }

    // Get all possible neighbours of a node based on the location on the grid
    public List<Node> GetNeighbours(Node node, Cell[,] grid)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                int neighbourX = node.position.x + x;
                int neighbourY = node.position.y + y;

                if (neighbourX < grid.GetLength(0) && neighbourY < grid.GetLength(1))
                {
                    neighbours.Add(new Node(new Vector2Int(neighbourX, neighbourY), null, grid[neighbourX, neighbourY].index));
                }
            }
        }

        return neighbours;
    }

    /// <summary>
    /// This is the Node class you can use this class to store calculated FScores for the cells of the grid, you can leave this as it is
    /// </summary>
    public class Node
    {
        public Vector2Int position; //Position on the grid
        public Node parent; //Parent Node of this node
        public int index;

        public float FScore { //GScore + HScore
            get { return GScore + HScore; }
        }
        public int GScore; //Current Travelled Distance
        public int HScore; //Distance estimated based on Heuristic

        public Node() { }
        public Node(Vector2Int position, Node parent, int index)
        {
            this.position = position;
            this.parent = parent;
            this.index = index;
        }
    }
}
