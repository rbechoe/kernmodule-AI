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
                    float dist = Vector3.Distance(hit.transform.position, PC.attacker.transform.position);
                    if (dist < closestDistance)
                    {
                        chosenObject = hit.gameObject;
                    }
                }
                
                // method used to calculate position behind cover compared to enemy position vs cover position
                // using local scale because all colliders are scaled this gives an accurate cover coordinate that is not squished against the wall
                float newX = 0;
                float newZ = 0;
                /*if (PC.attacker.transform.position.x > chosenObject.transform.position.x)
                {
                    newX = PC.attacker.transform.position.x - chosenObject.transform.position.x;
                    newX -= chosenObject.transform.localScale.x / 2f + 1;
                }
                else
                {
                    newX = PC.attacker.transform.position.x + chosenObject.transform.position.x;
                    newX += chosenObject.transform.localScale.x / 2f + 1;
                }
                if (PC.attacker.transform.position.z > chosenObject.transform.position.z)
                {
                    newZ = PC.attacker.transform.position.z - chosenObject.transform.position.z;
                    newZ -= chosenObject.transform.localScale.z / 2f + 1;
                }
                else
                {
                    newZ = PC.attacker.transform.position.z + chosenObject.transform.position.z;
                    newZ += chosenObject.transform.localScale.z / 2f + 1;
                }
                NMA.destination = new Vector3(newX, chosenObject.transform.position.y, newZ);*/
                NMA.destination = chosenObject.transform.position;
                Debug.Log(chosenObject.name);
                // TODO BUG: either overwrites received position or gets position of wrong object



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
