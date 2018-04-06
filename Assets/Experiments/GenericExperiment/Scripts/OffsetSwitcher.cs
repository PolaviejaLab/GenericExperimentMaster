using UnityEngine;
using System.Collections;

public class OffsetSwitcher : MonoBehaviour {

    public float initialOffset = 0;

	private float previous = -1; 

	private GameObject controller;

    private Vector3 initPosition;
    
	void Start () {

	}
	
	void Update () {
//		if (selectedOffset < -1) {
//			selectedOffset = 0;
//		}
		if (initialOffset != previous) {
			UpdateOffset ();
			previous = initialOffset;
		}
	}

	protected void UpdateOffset (){
		// method that will make the offset change, get the handcontroller to shift with the offset
		// use a find method 
		controller = GameObject.Find ("LeapHandController");
        initPosition = controller.transform.localPosition;
        controller.transform.localPosition = new Vector3(-initialOffset, 0, 0);
        

        Debug.Log("Changing offset: " + controller.transform.localPosition);
	}

    public void displaceHand(float displacement) {
        Vector3 displacement3 = new Vector3(-displacement, 0, 0);
        controller.transform.localPosition += displacement3;
    }

    public void noDisplacement()
    {
        controller.transform.localPosition = initPosition;
    }
}
