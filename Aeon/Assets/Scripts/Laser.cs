using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour {

    public Vector3 velocity;
    public Ship ship;
    private float maxLifetime = 10;
    private float currentLifetime = 0;
    public float damage;
    public enum eLaserType
    {
        Regular,
        Big
    }

    public eLaserType type;

	void Start () {
        if (PlayerManager.laserSoundCooldown <= 0 || !ship.AIControlled)
        {
            AudioSource sound = GetComponent<AudioSource>();
            if (sound != null)
            {
                sound.Play();
                PlayerManager.laserSoundCooldown = 0.2f;
            }
            
        }
	}
	
	// Update is called once per frame
	void Update () {
        gameObject.transform.position += velocity * Time.deltaTime;
        currentLifetime += Time.deltaTime;
        if (currentLifetime > maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Ship possibleTarget = col.gameObject.GetComponent<Ship>();
        if (possibleTarget != null)
        {
            if (possibleTarget == ship || possibleTarget.teamId == ship.teamId)
            {
                return;
            }
            else
            {
                if (possibleTarget.isAlive)
                {
                    possibleTarget.TakeDamage(damage, ship);
                    if (type != eLaserType.Big)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
