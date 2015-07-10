using UnityEngine;
using System.Collections;

public class GridInstantiator : MonoBehaviour {

	[SerializeField] protected Material[] gridColours;
	[SerializeField] protected int width,
								   height,
								   depth;

	[SerializeField] protected GameObject gridBlockPrefab;

	float offset = 1.2f;

	void Start () {
		for (int d = 0; d <= depth; d++){
			GameObject grid = new GameObject("Grid");
			grid.transform.parent = this.transform;

			for (float w = -width / 2; w <= (width - 1) / 2; w++){
				GameObject row = new GameObject("Row");
				row.transform.parent = grid.transform;

				for (float h = -height / 2; h <= (height - 1) / 2; h++){
					CreateBlock(row.transform, new Vector3(w * offset,
														   h * offset,
														   d * offset));
				}
			}
		}
	}

	void CreateBlock(Transform parent, Vector3 position){
		GameObject block = GameObject.Instantiate(gridBlockPrefab);
		gridBlockPrefab.GetComponent<MeshRenderer>().sharedMaterial = gridColours[Random.Range(0, gridColours.Length)];
		block.transform.parent = parent;
		block.name = "Block";

		block.transform.localPosition = position;
	}

	void Update () {

	}
}
