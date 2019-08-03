using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class PlayerManager : MonoBehaviour {
    public class Player
    {
        int playerId;
        public Player(int pId)
        {
            this.playerId = pId;
        }
    }

    public static float laserSoundCooldown = 0.2f;
    public static float explosionSoundCooldown = 0.2f;

    public List<Player> players;
    public int assignedPlayers;
    public int assignedShips;
    public List<Ship> ships;
    public AIShip allyShipPrefab;
    public PlayerController playerPrefab;
    public static List<Color> teamColours;
	// Use this for initialization
	void Start () {
        teamColours = new List<Color>();
        teamColours.Add(new Color(111/255f, 195/255f, 223/255f, 1));
        teamColours.Add(new Color(223/255f, 116/255f, 12/255f, 1));
    }

    public void CreateWorld(int numEnemies, int numAllies, int numPlayers)
    {
        for (int i = 0; i < numEnemies; i++)
        {
            AIShip newship = (AIShip)Instantiate(allyShipPrefab,
            new Vector3(Random.Range(-20, 20), Random.Range(20, 22)),
            transform.rotation);
            newship.teamId = 1;
        }

        for (int i = 0; i < numAllies; i++)
        {
            AIShip newship = (AIShip)Instantiate(allyShipPrefab,
            new Vector3(Random.Range(-20, 20), Random.Range(-20, -22)),
            transform.rotation);
            newship.teamId = 0;
        }

        bool first = true;
        for (int i = 0; i < numPlayers; i++)
        {
            PlayerController newship = (PlayerController)Instantiate(playerPrefab,
            new Vector3(Random.Range(-20, 20), Random.Range(-20, -22)),
            transform.rotation);
            if (first)
            {
                //newship.teamId = 1;
                //newship.transform.position = new Vector3(newship.transform.position.x,
                //    20, 0);
                first = false;
            }
        }

        if (numPlayers >= 2)
        {
            Camera firstCam = GameObject.Find("camera0").GetComponent<Camera>();
            firstCam.rect = new Rect(0, 0, 0.5f, 1);

            Camera secondCam = GameObject.Find("camera1").GetComponent<Camera>();
            secondCam.rect = new Rect(0.5f, 0, 0.5f, 1);
        }
    }

    void Awake()
    {
        assignedPlayers = 0;
        players = new List<Player>();
        players.Add(new Player(0));
        players.Add(new Player(1));
        ships = new List<Ship>();
    }

    public int AssignShip()
    {
        assignedShips++;
        return assignedShips;
    }

    public int AssignPlayer()
    {
        if (assignedPlayers < players.Count)
        {
            assignedPlayers++;
            //adjust the cameras
            if (assignedPlayers == 2)
            {

            }
            else if (assignedPlayers == 3)
            {

            }
            else if (assignedPlayers == 4)
            {

            }
            return assignedPlayers-1;
        }
        else
        {
            return -1;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (laserSoundCooldown >= 0)
        {
            laserSoundCooldown -= Time.deltaTime;
        }
        if (explosionSoundCooldown >= 0)
        {
            explosionSoundCooldown -= Time.deltaTime;
        }
    }
}


