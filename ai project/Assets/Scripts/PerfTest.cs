using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerfTest : MonoBehaviour
{
    const int SIZE = 4096;
    System.DateTime startTime, endTime;
    void Start()
    {
        // prep data
        int[] data = new int[SIZE * SIZE];

        // start meting
        startTime = System.DateTime.Now;
        print("Started at: " + Time.time);

        // do stuff
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                int index = i * j;
                data[index] = j * i + index;
            }
        }

        // end meting
        endTime = System.DateTime.Now;
        System.TimeSpan duration = endTime - startTime;
        print("Ended at: " + duration);

        // print result
        print("Did task in " + (endTime - startTime));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
