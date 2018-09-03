using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class PlayerManager : MonoBehaviour {
    public class Player
    {
        int playerId;
        public UnityEngine.KeyCode forward;
        public UnityEngine.KeyCode left;
        public UnityEngine.KeyCode right;
        public Player(int pId, UnityEngine.KeyCode frwd, UnityEngine.KeyCode lt, UnityEngine.KeyCode rt)
        {
            this.playerId = pId;
            this.forward = frwd;
            this.left = lt;
            this.right = rt;
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
        players.Add(new Player(0, KeyCode.W, KeyCode.A, KeyCode.D));
        players.Add(new Player(1, KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.RightArrow));
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


