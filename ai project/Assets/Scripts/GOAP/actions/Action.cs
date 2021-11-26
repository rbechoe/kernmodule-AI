using UnityEngine;
using System.Collections;

public class Action : MonoBehaviour
{
    public string actionName;
    public int requiredItem;
    public int itemId;
    public int actionCost;
    public bool hasRequirement;

    // used for ASTAR
    public float FScore
    { //GScore + HScore
        get { return GScore + HScore; }
    }
    public int GScore; //Current Travelled Distance
    public int HScore; //Distance estimated based on Heuristic
    public Action parent;
}
