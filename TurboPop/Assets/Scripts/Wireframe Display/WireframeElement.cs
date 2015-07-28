using UnityEngine;
using System.Collections;

public class WireframeElement : MonoBehaviour {

	[SerializeField] protected Material emptyMaterial,
										occupiedMaterial;

	MeshRenderer meshRenderer;

	void Awake(){
		meshRenderer = GetComponent<MeshRenderer>();
	}

	public void SetOccupied(){
		meshRenderer.sharedMaterial = occupiedMaterial;
	}

	public void SetEmpty(){
		meshRenderer.sharedMaterial = emptyMaterial;
	}
}
