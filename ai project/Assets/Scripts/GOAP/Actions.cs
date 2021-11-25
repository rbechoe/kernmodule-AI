using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Actions : MonoBehaviour
{
    public List<GameObject> availableActions = new List<GameObject>();
    // TODO make scriptable objects

    // select random node

    // calculate distances between nodes

    // calculate individual node cost

    // use A* to calculate routes

    private void Start()
    {
        availableActions = GameObject.FindObjectsOfType<IAction>();
    }
}