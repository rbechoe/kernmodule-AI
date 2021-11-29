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
    List<GameObject> waypoints = new List<GameObject>();
    float distanceFromDest;
    bool followingPlan;
    bool healOnDone;
    bool restOnDone;
    bool swordOnDone;
    bool hasWeapon;
    float idleTimer;
    float attackCd;

    [Header("Predefined Actions")]
    public Action healing;
    public Action resting;
    public Action smithing;
    public float eatTimer = 10;
    public float sleepTimer = 20;
    public float restFromHeal = 20;
    public float attackCdReset = 2;
    public float waitTimer = 3;
    public float waitReset = 3;

    [Header("Agent Settings")]
    public GameObject healthBar;
    public GameObject waypointParent;
    public TextMeshPro activityText;
    public float mapSize = 50;
    public float killIncrement = 1;
    public float hungerIncrement = 0.1f;
    public float tiredIncrement = 0.1f;
    public float health = 100;
    public float noDesireWeight = 20;
    public float aggroRange = 5;
    public float attackRange = 2;

    [Header("Agent Desires")]
    [Range(0, 100)]
    public float desireToKill = 0;
    [Range(0, 100)]
    public float desireToEat = 0;
    [Range(0, 100)]
    public float desireToRest = 0;

    void Start()
    {
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
        distanceFromDest = Vector3.Distance(transform.position, NMA.destination);

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

        UpdateDesires();
    }

    void UpdateDesires()
    {
        desireToRest += (tiredIncrement + (1 - health / 100)) * Time.deltaTime;
        desireToEat += hungerIncrement * Time.deltaTime;
        if (Vector3.Distance(transform.position, player.transform.position) < aggroRange)
        {
            desireToKill += killIncrement;
            
            if (desireToKill > 80)
            {
                MurderousPlan();
            }
        }
        else
        {
            desireToKill -= killIncrement / 10f * Time.deltaTime;
        }

        desireToRest = Mathf.Clamp(desireToRest, 0, 100);
        desireToEat = Mathf.Clamp(desireToEat, 0, 100);
        desireToKill = Mathf.Clamp(desireToKill, 0, 100);
        health = Mathf.Clamp(health, 0, 100);
    }

    void UpdatePlan()
    {
        if (NMA.destination != AP.pathToActions[0]) NMA.destination = AP.pathToActions[0];

        if (Vector3.Distance(transform.position, AP.pathToActions[0]) < 1.5f)
        {
            if (waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
                activityText.text = "Doing " + AP.routeToGoal[0].actionName;
                return;
            }

            Debug.Log("Completed: " + AP.routeToGoal[0].actionName);
            AP.pathToActions.RemoveAt(0);
            AP.routeToGoal.RemoveAt(0);
            waitTimer = waitReset;

            if (AP.pathToActions.Count > 0)
            {
                NMA.destination = AP.pathToActions[0];
            }
            else
            {
                followingPlan = false;
                if (healOnDone)
                {
                    health = 100;
                    desireToEat = 0;
                    healOnDone = false;
                }
                if (restOnDone)
                {
                    desireToRest = 0;
                    health += restFromHeal;
                    desireToRest -= restFromHeal;
                    restOnDone = false;
                }
                if (swordOnDone)
                {
                    hasWeapon = true;
                    swordOnDone = false;
                }
            }
        }
    }

    void MurderousPlan()
    {
        if (!hasWeapon && !followingPlan)
        {
            activityText.text = "Planning to murder...";
            AP.SelectGoal(smithing);
            swordOnDone = true;
        }

        if (hasWeapon)
        {
            if (Vector3.Distance(transform.position, player.transform.position) < aggroRange)
            {
                activityText.text = "Desire to kill...";
                NMA.destination = player.transform.position;
            }

            if (Vector3.Distance(transform.position, player.transform.position) < attackRange)
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

    void GenerateDestination()
    {
        // set action based on desires
        float choice = Random.Range(0, noDesireWeight + desireToEat + desireToRest);
        if (choice > noDesireWeight && choice < noDesireWeight + desireToEat)
        {
            activityText.text = "Planning to heal...";
            AP.SelectGoal(healing);
            NMA.destination = AP.pathToActions[0];
            idleTimer = eatTimer;
            followingPlan = true;
            healOnDone = true;
            return;
        }

        if (choice > noDesireWeight + desireToEat && choice < noDesireWeight + desireToEat + desireToRest)
        {
            activityText.text = "Planning to rest...";
            AP.SelectGoal(resting);
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
