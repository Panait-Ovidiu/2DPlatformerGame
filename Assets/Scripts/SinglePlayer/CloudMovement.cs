using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    [SerializeField] private Transform mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if ((transform.position.x - mainCamera.transform.position.x) < -19f)
        {
            transform.position = new Vector3(transform.position.x + 42f, transform.position.y, transform.position.z);
        }
        transform.position = new Vector3(transform.position.x - (0.3f * Time.deltaTime), transform.position.y, transform.position.z);
    }
}
