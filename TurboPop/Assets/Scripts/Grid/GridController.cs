using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridController : MonoBehaviour {

	static GridController instance;
	static int segmentCount = 10;

	List<GridSegment> gridSegments;

	public static GridController Instance{
		get {
			return instance;
		}
	}

	public static int SegmentCount {
		get {
			return segmentCount;
		}
	}

	public void AddSegment(GridSegment segment){
		gridSegments.Add(segment);
	}

	void Awake(){
		if (instance == null){
			instance = this;
			gridSegments = new List<GridSegment>();
		}
	}

	void Start () {

	}

	void Update () {

	}
}
