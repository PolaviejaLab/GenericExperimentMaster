using UnityEngine;
using System.Collections;


/**
 * Swaps one material for another
 */
public class MaterialChanger : MonoBehaviour {

	public int activeMaterial;
	public Material[] materials;
	
	private int oldMaterial;


	void Start () {
		oldMaterial = activeMaterial + 1;
	}
	

	/**
	 * Detect change in activation state and swap material if required.
	 */
	void Update () {
		if(oldMaterial != activeMaterial) {
			MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
			
			if(activeMaterial < materials.Length)
				meshRenderer.material = materials[activeMaterial];
			oldMaterial = activeMaterial;
		}
	}
}
