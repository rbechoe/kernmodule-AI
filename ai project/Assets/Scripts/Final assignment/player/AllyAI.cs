using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class AllyAI : MonoBehaviour
{
    NavMeshAgent NMA;
    ActionPlanner AP;
    GameObject player;
    PlayerController PC;
    TrajectoryCalculator TC;
    public Inventory inventory;
    public GameObject destObj;
    public TextMeshPro activityText;
    bool followingPlan;
    bool inCover;
    bool isHidden;
    float idleTimer;
    float waitTimer = 3;
    float bombCd = 5;
    float stalkRange = 5;
    float pathCd = 0;

    [Header("Agent Inventory")]
    public ItemList[] items;
    public int[] amounts;

    void Start()
    {
        inventory = new Inventory();
        NMA = gameObject.GetComponent<NavMeshAgent>();
        AP = gameObject.GetComponent<ActionPlanner>();
        player = GameObject.FindGameObjectWithTag("Player");
        PC = player.GetComponent<PlayerController>();
        TC = gameObject.GetComponent<TrajectoryCalculator>();
    }

    // Update is called once per frame
    void Update()
    {
        items = inventory.GetKeys();
        amounts = inventory.GetValues();

        if (followingPlan)
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

                waitTimer = AP.waitTimePerAction[0];

                // update inventory
                if (AP.actionsToGoal.Count > 0 && AP.actionsToGoal[0] != null)
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
                }
            }

            return;
        }

        if (!PC.attacked)
        {
            if (Vector3.Distance(player.transform.position, transform.position) > stalkRange)
            {
                NMA.destination = player.transform.position;
                inCover = false;
            }
            else
            {
                NMA.destination = transform.position;
                inCover = false;
            }
        }
        else
        {
            Vector3 enemyPos = PC.attacker.transform.position;
            // if not covered and calculating new destination for cover and can see enemy, go hide!
            if (pathCd <= 0 && !inCover)
            {
                Debug.Log("Searching cover!");
                Collider[] hits = Physics.OverlapSphere(transform.position, 100f);
                GameObject chosenObject = null;
                float closestDistance = float.MaxValue;
                foreach(Collider hit in hits)
                {
                    if (hit.GetComponent<NavMeshAgent>() || hit.CompareTag("Player"))
                    {
                        continue; // skip objects that should not be used for hiding
                    }
                    float dist = Vector3.Distance(hit.transform.position, enemyPos);
                    if (dist < closestDistance)
                    {
                        chosenObject = hit.gameObject;
                    }
                }

                // method used to calculate position behind cover compared to enemy position vs cover position
                // reference: https://gyazo.com/ce97aeb37c1efc8a0b46dc98043c9778
                Vector3 newPos = chosenObject.transform.position;
                float newX = newPos.x;
                float newZ = newPos.z;
                Vector3 diffA = (PC.attacker.transform.position - newPos) / 2f;
                Vector3 diffB = (newPos - PC.attacker.transform.position) / 2f;
                // enemy is top right
                if (enemyPos.x > newX && enemyPos.z > newZ)
                {
                    newX -= Mathf.Abs(diffA.x);
                    newZ -= Mathf.Abs(diffA.z);
                }
                // enemy is bottom left
                if (enemyPos.x < newX && enemyPos.z < newZ)
                {
                    newX += Mathf.Abs(diffB.x);
                    newZ += Mathf.Abs(diffB.z);
                }
                // enemy is bottom right
                if (enemyPos.x > newX && enemyPos.z < newZ)
                {
                    newX -= Mathf.Abs(diffA.x);
                    newZ += Mathf.Abs(diffB.z);
                }
                // enemy is top left
                if (enemyPos.x < newX && enemyPos.z > newZ)
                {
                    newX += Mathf.Abs(diffB.x);
                    newZ -= Mathf.Abs(diffA.z);
                }
                newPos = new Vector3(newX, chosenObject.transform.position.y, newZ);
                NMA.destination = newPos;
                inCover = true;

                pathCd = 5;
            }
            else
            {
                pathCd -= Time.deltaTime;
            }

            RaycastHit lineHit;
            isHidden = Physics.Linecast(transform.position, enemyPos, out lineHit);
            //Debug.Log(lineHit.collider.name);

            // if ally is at hiding position and cannot see the enemy then it can start throwing smoke bomb(s)
            if (inCover && isHidden && Vector3.Distance(transform.position, NMA.destination) < 2)
            {
                if (!inventory.HasRequirement(ItemList.Smoke_Bomb, 1))
                {
                    // Make action for collecting bombs
                    GameObject actionObj = new GameObject();
                    actionObj.AddComponent<Action>();
                    actionObj.transform.position = transform.position;
                    Action collectBomb = actionObj.GetComponent<Action>();
                    collectBomb.hasRequirement = true;
                    collectBomb.requiredAmount = 3;
                    collectBomb.requiredItem = ItemList.Smoke_Bomb;
                    collectBomb.givenItem = ItemList.Empty;
                    collectBomb.givenAmount = 0;

                    AP.SelectGoal(collectBomb, inventory);
                    NMA.destination = AP.pathToActions[0];
                    waitTimer = AP.waitTimePerAction[0];
                    followingPlan = true;

                    Destroy(actionObj);
                }
                else
                {
                    if (idleTimer < 0)
                    {
                        // throw bomb to enemy
                        TC.LaunchProjectile(PC.attacker);
                        inventory.RemoveFromInventory(ItemList.Smoke_Bomb, 1);
                        idleTimer = bombCd;
                    }
                    else
                    {
                        idleTimer -= Time.deltaTime;
                    }
                }
            }
        }

        destObj.transform.position = NMA.destination;
    }
}
