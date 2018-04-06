using UnityEngine;
using System.Collections;

public class SimpleCollision : MonoBehaviour {

	public WaveController waveController;

	public WaveEvents triggerEvent;
	public GameObject[] objects;
    public float probability;

	public bool CompareByName = false;	

	void OnTriggerStay(Collider col)
	{
        //   Debug.Log(col.name);
        probability = Random.Range(0.00f, 1.00f);
        if (col.name == "HandContainer"){
			if(objects.Length == 0) {
                waveController.HandleEvent(triggerEvent);
			} else {
				for(int i = 0; i < objects.Length; i++) {
					if(CompareByName) {
						if(col.gameObject.name == objects[i].name)
                            waveController.HandleEvent(triggerEvent);
					} else {
						if(col.gameObject == objects[i])
                            waveController.HandleEvent(triggerEvent);
					}
				}
			}
            waveController.randomProbability = probability;
		}
	}	
}
