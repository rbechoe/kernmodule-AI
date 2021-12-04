using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    NavMeshAgent NMA;
    ActionPlanner AP;
    GameObject player;
    EnemyUtilitySystem EUS;
    List<GameObject> waypoints = new List<GameObject>();
    public Inventory inventory;
    float distanceFromDest;
    bool followingPlan;
    bool healOnDone;
    bool restOnDone;
    float idleTimer;
    float attackCd;
    float waitTimer = 3;

    [Header("Predefined Actions")]
    public Action healing;
    public Action resting;
    public Action smithing;
    public float eatTimer = 10;
    public float sleepTimer = 20;
    public float restFromHeal = 20;
    public float attackCdReset = 2;
    public float waitReset = 3;

    [Header("Agent Settings")]
    public GameObject healthBar;
    public GameObject waypointParent;
    public TextMeshPro activityText;
    public float mapSize = 50;

    [Header("Agent Inventory")]
    public ItemList[] items;
    public int[] amounts;

    void Start()
    {
        inventory = new Inventory();
        EUS = new EnemyUtilitySystem(this);
        NMA = gameObject.GetComponent<NavMeshAgent>();
        AP = gameObject.GetComponent<ActionPlanner>();
        player = GameObject.FindGameObjectWithTag("Player");
        
        foreach (Transform child in waypointParent.GetComponentsInChildren<Transform>())
        {
            waypoints.Add(child.gameObject);
        }

        GenerateDestination();
    }

    void Update()
    {
        items = inventory.GetKeys();
        amounts = inventory.GetValues();
        distanceFromDest = Vector3.Distance(transform.position, NMA.destination);
        EUS.UpdateUtilities(Vector3.Distance(transform.position, player.transform.position));

        if (idleTimer > 0 && !followingPlan)
        {
            idleTimer -= Time.deltaTime;
        }

        if (distanceFromDest < 1.5f && !followingPlan && idleTimer <= 0)
        {
            GenerateDestination();
        }

        if (followingPlan)
        {
            UpdatePlan();
        }
    }

    void UpdatePlan()
    {
        if (NMA.destination != AP.pathToActions[0]) NMA.destination = AP.pathToActions[0];

        if (Vector3.Distance(transform.position, AP.pathToActions[0]) < 1.5f)
        {
            if (waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
                activityText.text = "Doing " + AP.actionsToGoal[0].actionName;
                return;
            }

            // TODO check if current goal is still reachable and if path is still most efficient 
            //AP.SelectGoal(AP.actionsToGoal[AP.actionsToGoal.Count - 1], this);
            // BUG: somehow doesnt get past 2nd smelting in order to smith sword

            waitTimer = AP.waitTimePerAction[0];
            Debug.Log("Completing: " + AP.actionsToGoal[0].actionName + " in aprox " + waitTimer + " seconds");

            // update inventory
            if (AP.actionsToGoal.Count > 0)
            {
                AP.actionsToGoal[0].PerformAction(inventory);
            }

            AP.waitTimePerAction.RemoveAt(0);
            AP.pathToActions.RemoveAt(0);
            AP.actionsToGoal.RemoveAt(0);

            if (AP.pathToActions.Count > 0)
            {
                NMA.destination = AP.pathToActions[0];
                activityText.text = "Moving to do " + AP.actionsToGoal[0].actionName;
            }
            else
            {
                followingPlan = false;
                if (healOnDone)
                {
                    EUS.health = 100;
                    EUS.desireToEat = 0;
                    healOnDone = false;
                }
                if (restOnDone)
                {
                    EUS.desireToRest = 0;
                    EUS.health += restFromHeal;
                    EUS.desireToRest -= restFromHeal;
                    restOnDone = false;
                }
            }
        }
    }

    // Attack player if it meets requirements, otherwise try to meet requirements
    public void MurderousPlan()
    {
        if (!inventory.HasItem(ItemList.Iron_Sword) && !followingPlan)
        {
            activityText.text = "Planning to murder...";
            AP.SelectGoal(smithing, this);
            NMA.destination = AP.pathToActions[0];
            followingPlan = true;
            healOnDone = false;
            restOnDone = false;
        }

        if (inventory.HasItem(ItemList.Iron_Sword))
        {
            if (Vector3.Distance(transform.position, player.transform.position) < EUS.aggroRange)
            {
                activityText.text = "Desire to kill...";
                NMA.destination = player.transform.position;
            }

            if (Vector3.Distance(transform.position, player.transform.position) < EUS.attackRange)
            {
                if (attackCd < 0)
                {
                    activityText.text = "Attack!!";
                    attackCd = attackCdReset;
                    Debug.Log("Attacked player!");
                }
            }
        }
        
        if (attackCd > 0) attackCd -= Time.deltaTime;
    }

    // Generate destination based on desires and a bit of rng
    void GenerateDestination()
    {
        // set action based on desires
        float choice = Random.Range(0, EUS.noDesireWeight + EUS.desireToEat + EUS.desireToRest);
        if (choice > EUS.noDesireWeight && choice < EUS.noDesireWeight + EUS.desireToEat)
        {
            activityText.text = "Planning to heal...";
            AP.SelectGoal(healing, this);
            NMA.destination = AP.pathToActions[0];
            idleTimer = eatTimer;
            followingPlan = true;
            healOnDone = true;
            return;
        }

        if (choice > EUS.noDesireWeight + EUS.desireToEat && choice < EUS.noDesireWeight + EUS.desireToEat + EUS.desireToRest)
        {
            activityText.text = "Planning to rest...";
            AP.SelectGoal(resting, this);
            NMA.destination = AP.pathToActions[0];
            idleTimer = sleepTimer;
            followingPlan = true;
            restOnDone = true;
            return;
        }

        // choose new destination if desires are not met
        // 5% chance to pick a completely random position instead of continuing with the waypoint system
        Vector3 destination = Vector3.zero;
        if (Random.Range(0, 100) < 5)
        {
            destination = new Vector3(Random.Range(-mapSize, mapSize), 0, Random.Range(-mapSize, mapSize));
        }
        else
        {
            // add current waypoint to bottom of the list
            GameObject waypoint = waypoints[0];
            waypoints.RemoveAt(0);
            waypoints.Add(waypoint);
            destination = waypoints[0].transform.position;
            activityText.text = "Going to " + waypoints[0].name;
        }

        NMA.destination = destination;
        NavMeshPath NMP = new NavMeshPath();
        NMA.CalculatePath(destination, NMP);
        if (NMP.status == NavMeshPathStatus.PathPartial)
        {
            Debug.Log("Invalid destionation, calculating new one...");
            GenerateDestination();
        }
    }
}
