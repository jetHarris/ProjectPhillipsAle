using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour {
    public Laser laserPrefab;
    protected float firingTimer = 0.2f;
    protected float firingTimerReset;
    protected float maxSpeed = 5;
    protected float speedUpTime = 0.5f;
    protected float thrustingTime = 0;
    public float health = 30;
    public Vector2 forward;
    protected float speed = 5000f;
    protected float turnSpeed = 1;
    protected Vector2 acceleration;
    public Rigidbody2D myBody;
    public int teamId = 0;
    public int shipId;
    public bool AIControlled = true;
    protected bool fireRight = true;
    public bool isAlive
    {
        get { return health > 0; }
    }

    // Use this for initialization
    void Start () {
        firingTimerReset = firingTimer;
        acceleration = new Vector2();
        myBody = gameObject.GetComponent<Rigidbody2D>();
    }
	
	// Update is called once per frame
	void FixedUpdate () {
		
	}

    public virtual void TakeDamage(float damageAmount, Ship attacker)
    {
        if (isAlive)
        {
            health -= damageAmount;
            if (!isAlive)
            {
                SpriteRenderer art = gameObject.GetComponentInChildren<SpriteRenderer>();
                if (art != null)
                {  
                    art.color = new Color(94 / 255f, 79 / 255f, 79 / 255f, 150 / 255f);
                }
            }
        }
    }
}
