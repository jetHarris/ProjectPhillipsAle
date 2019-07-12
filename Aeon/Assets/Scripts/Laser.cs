using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour {

    public Vector3 velocity;
    public PlayerController ship;
    private float maxLifetime = 10;
    public float damage;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        gameObject.transform.position += velocity * Time.deltaTime;

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        PlayerController possibleTarget = col.gameObject.GetComponent<PlayerController>();
        if (possibleTarget != null)
        {
            if (possibleTarget == ship)
            {
                return;
            }
            else
            {
                possibleTarget.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}
