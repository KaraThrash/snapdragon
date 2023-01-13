using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayTimer : MonoBehaviour
{
    private float timeElapsed = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;
    }
}
