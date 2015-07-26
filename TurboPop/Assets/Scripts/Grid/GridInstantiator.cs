using UnityEngine;
using System.Collections;

public class GridInstantiator : MonoBehaviour {

	[SerializeField] protected Material[] gridColours;
	[SerializeField] protected Material[] particleColours;
	[SerializeField] protected Material deadCubeColour;

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

	public static float InitialDepth {
		get {
			return instance.initialDepth;
		}
	}

	public static float Offset {
		get {
			return instance.offset;
		}
	}

	public ParticleSystem GetParticlesForExplosion(Transform target, CubeColours colour){
		var ps = GameObject.Instantiate(explosionParticleSystem, target.position, target.rotation) as ParticleSystem;

		if (ps.GetComponent<Renderer>() == null){
			return null;
		}

		ps.GetComponent<Renderer>().material = particleColours[(int)colour];

		return ps;
	}

	public Material GetDeadCubeMaterial(){
		return deadCubeColour;
	}

	public Material GetGridColourForType(CubeColours colour){
		return gridColours[(int)colour];
	}

	void Awake(){
		if (instance == null){
			instance = this;
			widthValue = GridController.GridWidth;
			heightValue = GridController.GridHeight;
		}
	}

	public void CreateGrid(){
		var grid = GridController.Instance;

		for (int depth = 0; depth < GridController.SegmentCount; depth++){
			var segment = CreateGridSegment(depth);
			segment.transform.parent = grid.transform;
			grid.AddSegment(segment);
		}

		var bufferSegment = CreateGridSegment(GridController.SegmentCount);
		grid.SetBufferSegment(bufferSegment);

		grid.transform.position = new Vector3(grid.transform.position.x,
											  grid.transform.position.y,
											  initialDepth);
	}

	public CubeColours GetRandomCubeColour(){
		int color = Random.Range(0, gridColours.Length);
		return (CubeColours)color;
	}

	GridSegment CreateGridSegment(int depth){
		GameObject obj = new GameObject("Segment");
		GridSegment segment = obj.AddComponent<GridSegment>();

		for (float width = -widthValue / 2; width <= (widthValue - 1) / 2; width++){
			segment.AddSegmentRow(CreateGridRow(width, depth, obj));
		}

		segment.transform.localPosition = new Vector3(0, 0, depth * offset);

		return segment;
	}

	GridSegmentRow CreateGridRow(float width, int depth, GameObject grid){
		GameObject obj = new GameObject("Row");
		var row = obj.AddComponent<GridSegmentRow>();
		row.transform.parent = grid.transform;

		for (float height = -heightValue / 2; height <= (heightValue - 1) / 2; height++){
			var element = CreateElement(row.transform, new Vector3(height * offset,
												   			   	   width * offset,
												   			   	   0));
			element.transform.parent = row.transform;
			row.AddElement(element);
		}

		return row;
	}

	GridSegmentElement CreateElement(Transform parent, Vector3 position){
		GridSegmentElement block = GameObject.Instantiate(gridSegmentElementPrefab) as GridSegmentElement;

		block.InitializeElement(GetRandomCubeColour());
		block.transform.parent = parent;
		block.name = "Block";
		block.transform.localPosition = position;

		return block;
	}

}
