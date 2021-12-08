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
    bool followingPlan;
    float idleTimer;
    float waitTimer = 3;
    float bombCd = 2;
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
            }
            else
            {
                NMA.destination = transform.position;
            }
        }
        else
        {
            if (pathCd <= 0)
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
                    float dist = Vector3.Distance(hit.transform.position, PC.attacker.transform.position);
                    if (dist < closestDistance)
                    {
                        chosenObject = hit.gameObject;
                    }
                }
                
                // TODO cast raycast to see if it can see target, if yes find new destination if already at destination
                // method used to calculate position behind cover compared to enemy position vs cover position
                Vector3 newPos = chosenObject.transform.position;
                float newX = newPos.x;
                float newZ = newPos.z;
                Vector3 diffA = (PC.attacker.transform.position - newPos) / 2f;
                Vector3 diffB = (newPos - PC.attacker.transform.position) / 2f;
                // enemy is top right
                if (PC.attacker.transform.position.x > newX && PC.attacker.transform.position.z > newZ)
                {
                    newX -= Mathf.Abs(diffA.x);
                    newZ -= Mathf.Abs(diffA.z);
                }
                // enemy is bottom left
                if (PC.attacker.transform.position.x < newX && PC.attacker.transform.position.z < newZ)
                {
                    newX += Mathf.Abs(diffB.x);
                    newZ += Mathf.Abs(diffB.z);
                }
                // enemy is bottom right
                if (PC.attacker.transform.position.x > newX && PC.attacker.transform.position.z < newZ)
                {
                    newX -= Mathf.Abs(diffA.x);
                    newZ += Mathf.Abs(diffB.z);
                }
                // enemy is top left
                if (PC.attacker.transform.position.x < newX && PC.attacker.transform.position.z > newZ)
                {
                    newX += Mathf.Abs(diffB.x);
                    newZ -= Mathf.Abs(diffA.z);
                }
                newPos = new Vector3(newX, chosenObject.transform.position.y, newZ);
                NMA.destination = newPos;

                // TODO calculate path to see if position is reachable, otherwise pick new cover object

                pathCd = 5;
            }
            else
            {
                pathCd -= Time.deltaTime;
            }
        }

        destObj.transform.position = NMA.destination;
    }
}
