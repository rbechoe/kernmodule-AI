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

        Node startNode = nodes[startPos.x, startPos.y];
        Node endNode = nodes[endPos.x, endPos.y];

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode); // starting point for the pathfinding

        while (openSet.Count > 0)
        {
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

            // Generate path to follow by reversing it
            if (currentNode == endNode)
            {
                List<Vector2Int> path = new List<Vector2Int>();
                List<Node> nodePath = RetracePath(startNode, endNode);
                for (int i = 0; i < nodePath.Count; i++)
                {
                    path.Add(nodePath[i].position);
                }
                return path;
            }

            // Get all neighbours of the node
            foreach (Node neighbour in GetNeighbours(currentNode, nodes))
            {
                bool wallBlock = CrossPatternWallCheck(currentNode, neighbour, grid);

                // skip this iteration if node is unreachable or if we already checked it
                if (closedSet.Contains(neighbour) || wallBlock)
                {
                    continue;
                }

                // TODO make sure to exclude diagonal neighbours
                // use formula to calculate negatives or positives for the difference which can not be bigger than 1
                int costToNeighbour = currentNode.GScore + GetDistance(currentNode, neighbour);
                if (costToNeighbour < neighbour.GScore || !openSet.Contains(neighbour))
                {
                    neighbour.GScore = costToNeighbour;
                    neighbour.HScore = GetDistance(neighbour, endNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
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
        Node prevNode = new Node(); // used for debugging purposes

        while (currentNode != null)
        {
            if (!path.Contains(currentNode))
            {
                path.Add(currentNode);
                if (currentNode == startNode)
                {
                    break;
                }
                prevNode = currentNode;
                currentNode = currentNode.parent;
            }
            else
            {
                Debug.Log(prevNode.position + " triggered recursion with parent " + currentNode.position);
                break;
            }
        }
        path.Reverse();
        return path;
    }

    // Check for walls in a + shape from current node
    bool CrossPatternWallCheck(Node currentNode, Node neighbour, Cell[,] grid)
    {
        bool wallBlock = false;

        // check if current node has wall between neighbour
        if (currentNode.position.x < neighbour.position.x)
        {
            // neighbour is on the eastern side
            if (grid[currentNode.position.x, currentNode.position.y].HasWall(Wall.RIGHT))
            {
                wallBlock = true;
            }
        }
        if (currentNode.position.x > neighbour.position.x && !wallBlock)
        {
            // neighbour is on the western side
            if (grid[currentNode.position.x, currentNode.position.y].HasWall(Wall.LEFT))
            {
                wallBlock = true;
            }
        }
        if (currentNode.position.y < neighbour.position.y && !wallBlock)
        {
            // neighbour is on the northern side
            if (grid[currentNode.position.x, currentNode.position.y].HasWall(Wall.UP))
            {
                wallBlock = true;
            }
        }
        if (currentNode.position.y > neighbour.position.y && !wallBlock)
        {
            // neighbour is on the southern side
            if (grid[currentNode.position.x, currentNode.position.y].HasWall(Wall.DOWN))
            {
                wallBlock = true;
            }
        }

        return wallBlock;
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
                    Node neighbour = nodes[neighbourX, neighbourY];
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
