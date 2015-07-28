using UnityEngine;
using System.Collections;

public class WireframeController : MonoBehaviour {

	static WireframeController instance;

	WireframeElement[,] wireframeElements;

	public static WireframeController Instance{
		get {
			return instance;
		}
	}

	void Awake(){
		if (instance == null){
			instance = this;
		}
	}

	public void InitWireframe(float targetZ, float offset, int widthValue, int heightValue, GameObject wireframeGridSegment){
		wireframeElements = new WireframeElement[widthValue, heightValue];

		int x = 0;
		for (float width = -widthValue / 2; width <= (widthValue - 1) / 2; width++){
			int y = 0;

			for (float height = -heightValue / 2; height <= (heightValue - 1) / 2; height++){
				var segment = GameObject.Instantiate(wireframeGridSegment);
				segment.transform.position = new Vector3(height * offset, width * offset, 0);
				segment.transform.parent = transform;

				wireframeElements[x,y] = segment.GetComponent<WireframeElement>();
				wireframeElements[x,y].SetOccupied();

				y++;
			}

			x++;
		}

		transform.position = new Vector3(0, 0, targetZ - offset);

	}

	public void UpdateWireframe(){
		var frontSegment = GridController.Instance.GetFrontmostSegment();

		for (int i = 0; i < GridController.GridWidth; i++){
			var row = frontSegment.GetSegmentRowAtIndex(i);
			for (int j = 0; j < GridController.GridHeight; j++){
				if (row.GetSegmentElementAtIndex(j).Destroyed){
					wireframeElements[i,j].SetEmpty();
				}
				else{
					wireframeElements[i,j].SetOccupied();
				}
			}
		}
	}


}
