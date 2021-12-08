using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AllyAI : MonoBehaviour
{
    NavMeshAgent NMA;
    ActionPlanner AP;
    GameObject player;
    PlayerController PC;
    public Inventory inventory;
    public GameObject destObj;
    public GameObject bombPrefab;
    bool followingPlan;
    bool coverDest;
    float idleTimer;
    float waitTimer = 3;
    float bombCd = 5;
    float stalkRange = 5;
    float pathCd = 0;

    [Header("Agent Inventory")]
    public ItemList[] items;
    public int[] amounts;

    public bool hardlocked;

    void Start()
    {
        inventory = new Inventory();
        NMA = gameObject.GetComponent<NavMeshAgent>();
        AP = gameObject.GetComponent<ActionPlanner>();
        player = GameObject.FindGameObjectWithTag("Player");
        PC = player.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PC.attacked)
        {
            if (Vector3.Distance(player.transform.position, transform.position) > stalkRange)
            {
                NMA.destination = player.transform.position;
                coverDest = false;
            }
            else
            {
                NMA.destination = transform.position;
                coverDest = false;
            }
        }
        else
        {
            Vector3 enemyPos = PC.attacker.transform.position;
            // if not covered and calculating new destination for cover and can see enemy, go hide!
            if (pathCd <= 0 && !coverDest && !Physics.Linecast(transform.position, enemyPos) && !followingPlan)
            {
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
                coverDest = true;

                pathCd = 5;
            }
            else
            {
                pathCd -= Time.deltaTime;
            }

            // if ally is at hiding position and cannot see the enemy then it can start throwing smoke bomb(s)
            if (coverDest && Physics.Linecast(transform.position, enemyPos) && Vector3.Distance(transform.position, NMA.destination) < 1)
            {
                if (!inventory.HasItem(ItemList.Smoke_Bomb))
                {
                    // TODO give goal to find 3 bombs
                    AP.CollectSpecificItem(ItemList.Smoke_Bomb, 3, inventory);
                    followingPlan = true;
                }
                else
                {
                    if (idleTimer < 0)
                    {
                        // TODO throw bomb to enemy
                        // TODO https://www.youtube.com/watch?v=03GHtGyEHas
                        idleTimer = bombCd;
                        Debug.Log("Throwing bomb!");
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
