using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x < -8f)
        {
            transform.position = new Vector3(transform.position.x + 116f, transform.position.y, transform.position.z);
        }
        transform.position = new Vector3(transform.position.x - (0.3f * Time.deltaTime), transform.position.y, transform.position.z);
    }
}
