using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 10;
    public float rotSpeed = 100;
    void Start()
    {
        
    }

    void Update()
    {
        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");
        transform.position += transform.forward * ver * Time.deltaTime * speed;
        transform.eulerAngles += new Vector3(0, hor, 0) * Time.deltaTime * rotSpeed;
    }
}
