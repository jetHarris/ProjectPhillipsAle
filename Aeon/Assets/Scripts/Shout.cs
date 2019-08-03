using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shout : MonoBehaviour {

    const float MAX_LIVE_COUNTER = 2;
    float liveCounter = MAX_LIVE_COUNTER;
    Text theText;
	// Use this for initialization
	void Start () {
        theText = gameObject.GetComponentInChildren<Text>();
    }
	
	// Update is called once per frame
	void Update () {
        liveCounter -= Time.deltaTime;
        Vector3 pos = gameObject.transform.position;
        pos.y += 1;
        gameObject.transform.position = pos;
        theText.color = new Color(1, 1, 1, liveCounter / MAX_LIVE_COUNTER);
        if (liveCounter < 0)
        {
            Destroy(gameObject);
        }
	}
}
