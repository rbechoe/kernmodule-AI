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
    PlayerController PC;
    List<GameObject> waypoints = new List<GameObject>();
    Material healthMat;
    Inventory inventory;
    bool followingPlan;
    bool healOnDone;
    bool restOnDone;
    float idleTimer;
    float attackCd;
    float waitTimer = 3;
    float dazeTimer;
    float distanceFromDest;
    [HideInInspector]
    public bool attackPlayer;

    [Header("Predefined Actions")]
    public Action healing;
    public Action resting;
    public Action smithing;
    public float eatTimer = 10;
    public float sleepTimer = 20;
    public float restFromHeal = 20;
    public float attackCdReset = 2;
    public float dazeReset = 5;
    public float waitReset = 3;

    [Header("Agent Settings")]
    public GameObject healthBar;
    public GameObject waypointParent;
    public TextMeshPro activityText;

    // used for debugging purposes in the editor
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
            activityText.text = "?!?!?";
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
        // make sure destination is same of target for plan
        if (NMA.destination != AP.pathToActions[0]) NMA.destination = AP.pathToActions[0];

        // when in range of target
        if (Vector3.Distance(transform.position, AP.pathToActions[0]) < 1.5f)
        {
            // perform action
            if (waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
                activityText.text = "Doing " + AP.actionsToGoal[0].actionName;
                // TODO play sound effect
                return;
            }

            // TODO debug checking if goal is still most efficient
            //AP.SelectGoal(AP.endGoal, inventory);

            waitTimer = AP.waitTimePerAction[0];

            // complete action
            EUS.desireToRest += AP.actionsToGoal[0].actionCost;
            AP.CompleteStep(inventory);

            if (AP.pathToActions.Count > 0)
            {
                NMA.destination = AP.pathToActions[0];
                activityText.text = "Moving to do " + AP.actionsToGoal[0].actionName;
            }
            else
            {
                // final step of plan has been completed
                followingPlan = false;
                if (healOnDone)
                {
                    EUS.health = 100;
                    EUS.desireToEat = 0;
                    healOnDone = false;
                    activityText.text = "Finished eating";
                }
                if (restOnDone)
                {
                    EUS.desireToRest = 0;
                    EUS.health += restFromHeal;
                    EUS.desireToRest -= restFromHeal;
                    restOnDone = false;
                    activityText.text = "Finished resting";
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
        UtilityStatus choice = EUS.GetNewStatus();
        if (choice == UtilityStatus.Eating)
        {
            activityText.text = "Planning to heal...";
            AP.SelectGoal(healing, inventory);
            NMA.destination = AP.pathToActions[0];
            idleTimer = eatTimer;
            followingPlan = true;
            healOnDone = true;
            return;
        }

        if (choice == UtilityStatus.Resting)
        {
            activityText.text = "Planning to rest...";
            AP.SelectGoal(resting, inventory);
            NMA.destination = AP.pathToActions[0];
            idleTimer = sleepTimer;
            followingPlan = true;
            restOnDone = true;
            return;
        }

        float randomChoice = Random.Range(0, 100);
        if (randomChoice < 5)
        {
            activityText.text = "Time for diversity...";
            AP.SelectRandomGoal(inventory);
            NMA.destination = AP.pathToActions[0];
            idleTimer = sleepTimer;
            followingPlan = true;
            restOnDone = true;
            return;
        }

        UpdateWaypoints();
        NMA.destination = waypoints[0].transform.position;
        activityText.text = "Going to " + waypoints[0].name;
    }

    // add current waypoint to bottom of the list
    void UpdateWaypoints()
    {
        GameObject waypoint = waypoints[0];
        waypoints.RemoveAt(0);
        waypoints.Add(waypoint);
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
