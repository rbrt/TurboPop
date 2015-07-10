using UnityEngine;
using System.Collections;

public class GridInstantiator : MonoBehaviour {

	[SerializeField] protected Material[] gridColours;
	[SerializeField] protected Material[] particleColours;

	[SerializeField] protected int widthValue,
								   heightValue,
								   depthValue;

	[SerializeField] protected GameObject gridBlockPrefab;
	[SerializeField] protected ParticleSystem explosionParticleSystem;

	float offset = 1.2f;

	static GridInstantiator instance;

	public static GridInstantiator Instance {
		get {
			return instance;
		}
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
		for (int depth = 0; depth <= depthValue; depth++){
			CreateGridLayer(depth);
		}
	}

	void CreateGridLayer(int depth){
		GameObject grid = new GameObject("Grid");
		grid.transform.parent = this.transform;

		for (float width = -widthValue / 2; width <= (widthValue - 1) / 2; width++){
			CreateGridRow(width, depth, grid);
		}
	}

	void CreateGridRow(float width, int depth, GameObject grid){
		GameObject row = new GameObject("Row");
		row.transform.parent = grid.transform;

		for (float height = -heightValue / 2; height <= (heightValue - 1) / 2; height++){
			CreateBlock(row.transform, new Vector3(width * offset,
												   height * offset,
												   depth * offset));
		}
	}

	void CreateBlock(Transform parent, Vector3 position){
		GameObject block = GameObject.Instantiate(gridBlockPrefab);

		int color = Random.Range(0, gridColours.Length);
		block.GetComponent<GridElement>().Init((CubeColours)color);
		block.transform.parent = parent;
		block.name = "Block";
		block.transform.localPosition = position;
	}

	public void DetermineBlocksToDestroy(GridElement element){

	}










}
