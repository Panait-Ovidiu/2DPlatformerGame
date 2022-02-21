using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    [SerializeField] private Text cherriesText;
    private int cherries = 0;

    private void Start()
    {
        cherries = PlayerPrefs.GetInt("cherries");
        cherriesText.text = "" + cherries;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Cherry"))
        {
            Destroy(collision.gameObject);
            cherries++;
            cherriesText.text = "" + cherries;
            PlayerPrefs.SetInt("cherries", cherries);
        }
    }
}
