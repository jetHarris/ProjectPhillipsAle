using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour {

    public Vector3 velocity;
    public Ship ship;
    private float maxLifetime = 10;
    private float currentLifetime = 0;
    public float damage;
    public Rigidbody2D myBody;
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

    public void LateStart()
    {
        GetComponent<Rigidbody2D>().velocity = velocity;
    }
	
	// Update is called once per frame
	void Update () {
        //gameObject.transform.position += velocity * Time.deltaTime;
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
                    if (possibleTarget.HasFlag(Ship.eShipState.Repelling))
                    {
                        Vector2 normal = (myBody.position - possibleTarget.myBody.position).normalized;
                        myBody.velocity = myBody.velocity - (2 * normal * (Vector2.Dot(normal, myBody.velocity)));

                        float angle = Mathf.Atan2(myBody.velocity.y, myBody.velocity.x) * Mathf.Rad2Deg;
                        gameObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                        gameObject.transform.rotation = Quaternion.Euler(gameObject.transform.rotation.eulerAngles + new Vector3(0, 0, 90));

                        ship = possibleTarget;
                    }
                    else
                    {
                        possibleTarget.TakeDamage(damage, ship, new Vector2(transform.position.x, transform.position.y));
                        if (type != eLaserType.Big)
                        {
                            //instead of destroying just disable
                            //gameObject.GetComponent<Collider2D>().enabled = false;
                            //gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
                            Destroy(gameObject);
                        }
                    }

                }
            }
        }
    }
}
