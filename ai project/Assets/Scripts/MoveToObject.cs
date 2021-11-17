using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToObject : MonoBehaviour
{
    public GameObject target;
    public float speed;

    void Start()
    {
        
    }

    void Update()
    {
        transform.position += (target.transform.position - transform.position).normalized * Time.deltaTime * speed;
    }
}
