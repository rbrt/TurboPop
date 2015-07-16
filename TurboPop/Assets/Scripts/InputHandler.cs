using UnityEngine;
using System.Collections;

public class InputHandler : MonoBehaviour {

	[SerializeField] Camera cameraForInput;

	void Update () {
		if ( Input.GetMouseButtonDown(0)){
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast (ray, out hit, 100.0f)){
				var element = hit.collider.GetComponent<GridSegmentElement>();
				if (element != null){
					element.WasClicked();
					GridController.Instance.DestroyMatchedElements(element);
				}
			}
		}
	}
}
