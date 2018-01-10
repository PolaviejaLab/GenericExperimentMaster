using UnityEngine;
using System.Collections;

public class TableLights : MonoBehaviour {



//	GameObject[] lights;
	private GameObject light1;
	private GameObject light2;
	private GameObject light4;

	public bool isOn;

	// Use this for initialization
	void Start () {
		light1 = GameObject.Find ("TableLights1");
		light2 = GameObject.Find ("TableLights2");
		light4 = GameObject.Find ("TableLightsInitial");

		isOn = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (!isOn) {
			light1.SetActive (false);
			light2.SetActive (false);
			light4.SetActive (false);
		} else if (isOn) {
			light1.SetActive (true);
			light2.SetActive (true);
			light4.SetActive (true);
		}
	}
}
