using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamagable
{
    NavMeshAgent NMA;
    ActionPlanner AP;
    GameObject player;
    EnemyUtilitySystem EUS;
    List<GameObject> waypoints = new List<GameObject>();
    Material healthMat;
    PlayerController PC;
    public Inventory inventory;
    float distanceFromDest;
    bool followingPlan;
    bool healOnDone;
    bool restOnDone;
    float idleTimer;
    float attackCd;
    float waitTimer = 3;
    float dazeTimer;
    float dazeReset = 5;
    public bool attackPlayer;

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

    [Header("Agent Statistics")]
    [Range(0, 100)]
    public float hp;
    [Range(0, 100)]
    public float anger;
    [Range(0, 100)]
    public float hunger;
    [Range(0, 100)]
    public float fatigue;

    void Start()
    {
        inventory = new Inventory();
        EUS = new EnemyUtilitySystem(this);
        NMA = gameObject.GetComponent<NavMeshAgent>();
        AP = gameObject.GetComponent<ActionPlanner>();
        player = GameObject.FindGameObjectWithTag("Player");
        PC = player.GetComponent<PlayerController>();
        healthMat = healthBar.GetComponent<Renderer>().material;
        
        foreach (Transform child in waypointParent.GetComponentsInChildren<Transform>())
        {
            waypoints.Add(child.gameObject);
        }

        GenerateDestination();
    }

    void Update()
    {
        if (dazeTimer > 0)
        {
            dazeTimer -= Time.deltaTime;
            return;
        }

        UpdateEnemyStats();

        if (idleTimer > 0 && !followingPlan)
        {
            idleTimer -= Time.deltaTime;
        }

        if (attackPlayer || attackCd > 0)
        {
            MurderousPlan();
            return;
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

    void UpdateEnemyStats()
    {
        items = inventory.GetKeys();
        amounts = inventory.GetValues();
        hp = EUS.health;
        anger = EUS.desireToKill;
        hunger = EUS.desireToEat;
        fatigue = EUS.desireToRest;

        distanceFromDest = Vector3.Distance(transform.position, NMA.destination);
        EUS.UpdateUtilities(Vector3.Distance(transform.position, player.transform.position));

        healthMat.color = new Color(1 - EUS.health / 100f, EUS.health / 100f, 0);
        healthBar.transform.localScale = new Vector3(EUS.health / 100f, 1, 0.1f);
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

            waitTimer = AP.waitTimePerAction[0];

            // update inventory
            if (AP.actionsToGoal.Count > 0)
            {
                AP.actionsToGoal[0].PerformAction(inventory);
                EUS.desireToRest += AP.actionsToGoal[0].actionCost;
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
    void MurderousPlan()
    {
        NMA.speed = 5;
        if (attackCd > 0)
        {
            attackCd -= Time.deltaTime;
            return;
        }

        if (!inventory.HasItem(ItemList.Iron_Sword) && !followingPlan)
        {
            activityText.text = "Planning to murder...";
            AP.SelectGoal(smithing, inventory);
            NMA.destination = AP.pathToActions[0];
            followingPlan = true;
            healOnDone = false;
            restOnDone = false;
        }

        if (inventory.HasItem(ItemList.Iron_Sword))
        {
            if (Vector3.Distance(transform.position, player.transform.position) < EUS.attackRange)
            {
                if (attackCd <= 0)
                {
                    NMA.destination = transform.position;
                    activityText.text = "Attack!!";
                    attackCd = attackCdReset;
                    // TODO do more damage if weapon equiped
                    PC.TakeDamage(5, DamageType.RawDamage, gameObject);
                    Debug.Log("Attacked player!");
                }
            }
            else if (Vector3.Distance(transform.position, player.transform.position) < EUS.aggroRange)
            {
                activityText.text = "Desire to kill...";
                NMA.destination = player.transform.position;
            }
        }
    }

    // Generate destination based on desires and a bit of rng
    void GenerateDestination()
    {
        NMA.speed = 3.5f;

        // set action based on desires
        float choice = Random.Range(0, EUS.noDesireWeight + EUS.desireToEat + EUS.desireToRest);
        if (choice > EUS.noDesireWeight && choice < EUS.noDesireWeight + EUS.desireToEat && EUS.health < 70 && EUS.desireToEat > 10)
        {
            activityText.text = "Planning to heal...";
            AP.SelectGoal(healing, inventory);
            NMA.destination = AP.pathToActions[0];
            idleTimer = eatTimer;
            followingPlan = true;
            healOnDone = true;
            return;
        }

        if (choice > EUS.noDesireWeight + EUS.desireToEat && choice < EUS.noDesireWeight + EUS.desireToEat + EUS.desireToRest && EUS.desireToRest > 25)
        {
            activityText.text = "Planning to rest...";
            AP.SelectGoal(resting, inventory);
            NMA.destination = AP.pathToActions[0];
            idleTimer = sleepTimer;
            followingPlan = true;
            restOnDone = true;
            return;
        }

        // choose new destination if desires are not met
        // 1% chance to pick a completely random position instead of continuing with the waypoint system
        Vector3 destination = Vector3.zero;
        if (Random.Range(0, 100) < 1)
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
    
    public void TakeDamage(int amount, DamageType damageType, GameObject attacker)
    {
        switch (damageType)
        {
            case DamageType.RawDamage:
                EUS.health -= amount;
                break;

            case DamageType.Daze:
                dazeTimer = dazeReset;
                break;
        }
    }
}
