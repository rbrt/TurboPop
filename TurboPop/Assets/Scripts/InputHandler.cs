using UnityEngine;
using System.Collections;

public class InputHandler : MonoBehaviour {

	[SerializeField] Camera cameraForInput;

	void Update () {
		if ( Input.GetMouseButtonDown(0)){
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast (ray, out hit, 100.0f)){
				if (hit.collider.GetComponent<GridSegmentElement>() != null){
					hit.collider.GetComponent<GridSegmentElement>().WasClicked();
				}
			}
		}
	}
}
