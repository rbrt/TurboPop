using UnityEngine;
using System.Collections;

public class GridInstantiator : MonoBehaviour {

	[SerializeField] protected Material[] gridColours;
	[SerializeField] protected Material[] particleColours;

	protected int widthValue,
				  heightValue;

	[SerializeField] protected GridSegmentElement gridSegmentElementPrefab;
	[SerializeField] protected ParticleSystem explosionParticleSystem;

	float offset = 1.2f,
		  initialDepth = 3.5f;

	static GridInstantiator instance;

	public static GridInstantiator Instance {
		get {
			return instance;
		}
	}

	public static GridSegment CreateNewGridSegment(){
		return instance.CreateGridSegment(GridController.SegmentCount);
	}

	public ParticleSystem GetParticlesForExplosion(Transform target, CubeColours colour){
		var ps = GameObject.Instantiate(explosionParticleSystem, target.position, target.rotation) as ParticleSystem;
		ps.GetComponent<Renderer>().material = particleColours[(int)colour];

		return ps;
	}

	public Material GetGridColourForType(CubeColours colour){
		return gridColours[(int)colour];
	}

	void Awake(){
		if (instance == null){
			instance = this;
		}
	}

	void Start () {
		widthValue = GridController.GridWidth;
		heightValue = GridController.GridHeight;
		CreateGrid();
	}

	void CreateGrid(){
		var grid = GridController.Instance;

		for (int depth = 0; depth < GridController.SegmentCount; depth++){
			var segment = CreateGridSegment(depth);
			segment.transform.parent = grid.transform;
			grid.AddSegment(segment);
		}

		grid.transform.position = new Vector3(grid.transform.position.x,
											  grid.transform.position.y,
											  initialDepth);
	}

	GridSegment CreateGridSegment(int depth){
		GameObject obj = new GameObject("Segment");
		GridSegment segment = obj.AddComponent<GridSegment>();

		for (float width = -widthValue / 2; width <= (widthValue - 1) / 2; width++){
			segment.AddSegmentRow(CreateGridRow(width, depth, obj));
		}

		return segment;
	}

	GridSegmentRow CreateGridRow(float width, int depth, GameObject grid){
		GameObject obj = new GameObject("Row");
		var row = obj.AddComponent<GridSegmentRow>();
		row.transform.parent = grid.transform;

		for (float height = -heightValue / 2; height <= (heightValue - 1) / 2; height++){
			var element = CreateElement(row.transform, new Vector3(height * offset,
												   			   	   width * offset,
												   			   	   depth * offset));
			element.transform.parent = row.transform;
			row.AddElement(element);
		}

		return row;
	}

	GridSegmentElement CreateElement(Transform parent, Vector3 position){
		GridSegmentElement block = GameObject.Instantiate(gridSegmentElementPrefab) as GridSegmentElement;

		int color = Random.Range(0, gridColours.Length);
		block.Init((CubeColours)color);
		block.transform.parent = parent;
		block.name = "Block";
		block.transform.localPosition = position;

		return block;
	}

}
