using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private string parentName;

    private float startX;
    private bool isLeft;

    private bool isStatic = false;
    private float projectileLifeSec = 1.5f;
    private float projectileStartTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        startX = transform.position.x;
        projectileStartTime = Time.time + projectileLifeSec;
        if(transform.localScale.x == -1)
        {
            isLeft = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isLeft)
        {
            if (isStatic)
            {
                if (Time.time > projectileStartTime)
                {
                    projectileStartTime = Time.time + projectileLifeSec;
                    Destroy(gameObject);
                }
            }
            else
            {
                transform.position = new Vector3(transform.position.x - (10f * Time.deltaTime), transform.position.y, 0);
                if (startX - transform.position.x > 10f)
                {
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            if (isStatic)
            {
                if (Time.time > projectileStartTime)
                {
                    projectileStartTime = Time.time + projectileLifeSec;
                    Destroy(gameObject);
                }
            }
            else
            {
                transform.position = new Vector3(transform.position.x + (10f * Time.deltaTime), transform.position.y, 0);
                if (startX - transform.position.x < -10f)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            Destroy(gameObject);
        }
    }

    public void SetParent(string parent)
    {
        parentName = parent;
    }

    public string GetParent()
    {
        return parentName;
    }

    public void SetStatic()
    {
        isStatic = true;
    }

    public void SetDirection(bool isLeft)
    {
        this.isLeft = isLeft;
    }
}
