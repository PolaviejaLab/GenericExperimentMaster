using UnityEngine;
using System.Collections;

public class Debbuging : MonoBehaviour {

    public WaveController waveController;

	void Start () {
	}
	
	void Update () {
		if (Input.GetKeyDown (KeyCode.A))
            waveController.HandleEvent(WaveEvents.Wave_0);
		if (Input.GetKeyDown (KeyCode.B))
            waveController.HandleEvent(WaveEvents.Wave_1);
		if (Input.GetKeyDown (KeyCode.X))
            waveController.HandleEvent(WaveEvents.Wave_Initial);
	}
}
