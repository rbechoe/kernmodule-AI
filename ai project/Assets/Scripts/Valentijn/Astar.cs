using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Astar
{
    Node[,] nodes;
    int width, height;

    /// <summary>
    /// TODO: Implement this function so that it returns a list of Vector2Int positions which describes a path
    /// Note that you will probably need to add some helper functions
    /// from the startPos to the endPos
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="grid"></param>
    /// <returns></returns>
    public List<Vector2Int> FindPathToTarget(Vector2Int startPos, Vector2Int endPos, Cell[,] grid, int _width, int _height)
    {
        // TODO fill up nodes somewhere where the cells are filled and assign values to them!
        width = _width;
        height = _height;
        nodes = new Node[width, height];
        nodes.Initialize();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                nodes[x, y] = new Node(grid[x,y].gridPosition, null);
            }
        }

        Debug.Log("calculating path...");
        Node startNode = nodes[startPos.x, startPos.y];
        Node endNode = nodes[endPos.x, endPos.y];

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode); // starting point for the pathfinding

        int count = 900; // fail safe to break out of while loop
        while (openSet.Count > 0)
        {
            count--;
            if (count <= 0)
            {
                Debug.Log("something went wrong...");
                return null;
            }
            
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
                // TODO infinite recursion somehow?
                Debug.Log("Succesfully found a path!");
                //return null;
                List<Vector2Int> path = new List<Vector2Int>();
                List<Node> nodePath = RetracePath(startNode, endNode);
                for (int i = 0; i < nodePath.Count; i++)
                {
                    path.Add(nodePath[i].position);
                }
                return path;
            }

            //--- somewhere bugged in this foreach ---
            foreach (Node neighbour in GetNeighbours(currentNode, nodes))
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

        int count = 50;
        while (currentNode != startNode)
        {
            count--;
            if (count < 0)
            {
                currentNode = startNode;
            }
            Debug.Log(currentNode.position);
            path.Add(currentNode); // TODO <-- triggers run out of memory error
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
    public List<Node> GetNeighbours(Node node, Node[,] nodes)
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

                if (neighbourX < width && neighbourY < height && neighbourX >= 0 && neighbourY >= 0)
                {
                    //Debug.Log("x: " + neighbourX + " - " + width + " y: " + neighbourY + " - " + height);
                    Node neighbour = nodes[neighbourX, neighbourY];
                    neighbour.parent = node;
                    neighbours.Add(neighbour);
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

        public float FScore { //GScore + HScore
            get { return GScore + HScore; }
        }
        public int GScore; //Current Travelled Distance
        public int HScore; //Distance estimated based on Heuristic

        public Node() { }
        public Node(Vector2Int position, Node parent)
        {
            this.position = position;
            this.parent = parent;
        }
    }
}
