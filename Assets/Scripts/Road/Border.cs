using Assets.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Border : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision Enter: {collision.gameObject.name}");
        if (collision.gameObject.CompareTag("Player"))
        {
            var player = collision.gameObject;
            var rb = player.GetComponent<Rigidbody>();
            //rb.velocity = new Vector3(0, rb.velocity.y, rb.velocity.z);
            player.GetComponentInParent<PlayerController>().ChangeTrack(MoveDirection.None);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Collision Enter 2D: {collision.gameObject.name}");

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger Enter {other.name}");
    }
}
