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

    public List<Player> players;
    public int assignedPlayers;
	// Use this for initialization
	void Start () {

    }

    void Awake()
    {
        assignedPlayers = 0;
        players = new List<Player>();
        players.Add(new Player(0));
        players.Add(new Player(1));
    }

    public int AssignPlayer()
    {
        if (assignedPlayers < players.Count)
        {
            assignedPlayers++;
            return assignedPlayers-1;
        }
        else
        {
            return -1;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}


